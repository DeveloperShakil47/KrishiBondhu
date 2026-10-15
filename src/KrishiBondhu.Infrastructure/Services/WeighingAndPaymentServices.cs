using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KrishiBondhu.Application.Common.Interfaces;
using KrishiBondhu.Application.Common.Models;
using KrishiBondhu.Application.DTOs;
using KrishiBondhu.Application.Interfaces.Repositories;
using KrishiBondhu.Application.Interfaces.Services;
using KrishiBondhu.Domain.Entities;
using KrishiBondhu.Domain.Enums;
using KrishiBondhu.Domain.Exceptions;
using KrishiBondhu.Infrastructure.Data;

namespace KrishiBondhu.Infrastructure.Services;

public class WeighingService : IWeighingService
{
    private readonly IWeighingRecordRepository _weighingRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogger _auditLogger;
    private readonly INotificationService _notificationService;

    public WeighingService(
        IWeighingRecordRepository weighingRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditLogger auditLogger,
        INotificationService notificationService)
    {
        _weighingRepository = weighingRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _auditLogger = auditLogger;
        _notificationService = notificationService;
    }

    public async Task<Result<WeighingRecordDto>> RecordWeighingAsync(RecordWeighingDto dto, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        var order = await _orderRepository.GetWithFullDetailsAsync(dto.OrderId, cancellationToken);
        if (order == null)
        {
            return Result<WeighingRecordDto>.Failure("অর্ডার পাওয়া যায়নি।");
        }

        if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
        {
            return Result<WeighingRecordDto>.Failure("সম্পন্ন বা বাতিল অর্ডারে নতুন ওজন পরিমাপ যোগ করা যাবে না।");
        }

        try
        {
            var existingRecords = await _weighingRepository.GetRecordsForOrderAsync(order.Id, cancellationToken);
            var sequence = existingRecords.Count + 1;

            var record = WeighingRecord.Create(
                dto.OrderId,
                dto.GrossWeight,
                dto.TareWeight,
                dto.Unit,
                dto.CollectionPointName,
                dto.District,
                dto.Upazila,
                currentUserId,
                _currentUserService.FullName ?? _currentUserService.Username,
                dto.ScaleIdentifier,
                dto.ScaleCalibrationStatus,
                dto.Remarks);

            record.SequenceNumber = sequence;
            record.ReceiptPhotoUrl = dto.ReceiptPhotoUrl;

            // If recorded by WeighingOperator or Farmer/Buyer
            if (_currentUserService.Role == UserRole.Farmer)
            {
                record.IsFarmerConfirmed = true;
                record.FarmerConfirmedAtUtc = DateTime.UtcNow;
            }
            else if (_currentUserService.Role == UserRole.Buyer)
            {
                record.IsBuyerConfirmed = true;
                record.BuyerConfirmedAtUtc = DateTime.UtcNow;
            }

            await _weighingRepository.AddAsync(record, cancellationToken);

            // Update order status to DeliveredPendingConfirmation if it was AwaitingDelivery
            if (order.Status == OrderStatus.AwaitingDelivery || order.Status == OrderStatus.OfferAccepted)
            {
                order.Status = OrderStatus.DeliveredPendingConfirmation;
                order.DeliveredAtUtc = DateTime.UtcNow;
                await _orderRepository.UpdateAsync(order, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditLogger.LogAsync("WeighingRecord", record.Id.ToString(), "RecordWeighing",
                $"Recorded Gross: {record.GrossWeight}, Tare: {record.TareWeight}, Net: {record.NetWeight} {record.Unit}", cancellationToken);

            // Notify both parties
            if (order.FarmerProfile != null)
            {
                await _notificationService.SendNotificationAsync(
                    order.FarmerProfile.UserId,
                    "নতুন ওজন রেকর্ড যুক্ত হয়েছে",
                    "New Weighing Recorded",
                    $"অর্ডার #{order.OrderNumber}-এ মোট ওজন {record.GrossWeight} এবং নিট ওজন {record.NetWeight} {record.Unit} রেকর্ড করা হয়েছে। অনুগ্রহ করে যাচাই ও অনুমোদন করুন।",
                    $"Weighing recorded for Order #{order.OrderNumber}. Net Weight: {record.NetWeight} {record.Unit}.",
                    "Weighing",
                    $"/orders/{order.Id}",
                    cancellationToken);
            }

            return Result<WeighingRecordDto>.Success(MapToDto(record), "ওজন পরিমাপ সফলভাবে সংরক্ষিত হয়েছে। উভয় পক্ষের অনুমোদনের অপেক্ষায় রয়েছে।");
        }
        catch (InvalidWeightException ex)
        {
            return Result<WeighingRecordDto>.Failure(ex.Message);
        }
    }

    public async Task<Result> ConfirmWeighingAsync(Guid weighingRecordId, bool isAgreed, string? notes = null, CancellationToken cancellationToken = default)
    {
        var record = await _weighingRepository.GetByIdAsync(weighingRecordId, cancellationToken);
        if (record == null) return Result.Failure("ওজন পরিমাপের রেকর্ড পাওয়া যায়নি।");

        var order = await _orderRepository.GetWithFullDetailsAsync(record.OrderId, cancellationToken);
        if (order == null) return Result.Failure("সম্পর্কিত অর্ডার পাওয়া যায়নি।");

        var userRole = _currentUserService.Role;
        var userId = _currentUserService.UserId;

        if (userRole == UserRole.Farmer)
        {
            if (isAgreed)
            {
                record.IsFarmerConfirmed = true;
                record.FarmerConfirmedAtUtc = DateTime.UtcNow;
            }
            else
            {
                record.Status = WeighingStatus.Disputed;
            }
        }
        else if (userRole == UserRole.Buyer)
        {
            if (isAgreed)
            {
                record.IsBuyerConfirmed = true;
                record.BuyerConfirmedAtUtc = DateTime.UtcNow;
            }
            else
            {
                record.Status = WeighingStatus.Disputed;
            }
        }
        else if (userRole == UserRole.Administrator || userRole == UserRole.WeighingOperator)
        {
            // Operator / Admin direct confirmation
            if (isAgreed)
            {
                record.IsFarmerConfirmed = true;
                record.IsBuyerConfirmed = true;
            }
            else
            {
                record.Status = WeighingStatus.Disputed;
            }
        }

        // When both parties have confirmed, mark as Confirmed and update Order's VerifiedNetQuantityKg
        if (record.IsFarmerConfirmed && record.IsBuyerConfirmed)
        {
            record.Status = WeighingStatus.Confirmed;
            order.VerifiedNetQuantityKg = record.NetWeightInKg;

            // Transition order to PaymentPending if waiting
            if (order.Status == OrderStatus.DeliveredPendingConfirmation || order.Status == OrderStatus.AwaitingDelivery)
            {
                order.Status = OrderStatus.PaymentPending;
            }

            await _orderRepository.UpdateAsync(order, cancellationToken);
        }

        await _weighingRepository.UpdateAsync(record, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync("WeighingRecord", record.Id.ToString(), "ConfirmWeighing",
            $"Confirmation update: Farmer={record.IsFarmerConfirmed}, Buyer={record.IsBuyerConfirmed}, Status={record.Status}", cancellationToken);

        return Result.Success(record.Status == WeighingStatus.Confirmed
            ? "উভয় পক্ষ কর্তৃক ওজন নিশ্চিত হয়েছে এবং চূড়ান্ত বিলিং পরিমাণ হালনাগাদ হয়েছে।"
            : "আপনার মতামত সংরক্ষিত হয়েছে।");
    }

    public async Task<IReadOnlyList<WeighingRecordDto>> GetWeighingHistoryForOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var records = await _weighingRepository.GetRecordsForOrderAsync(orderId, cancellationToken);
        return records.Select(MapToDto).ToList();
    }

    private static WeighingRecordDto MapToDto(WeighingRecord w)
    {
        return new WeighingRecordDto
        {
            Id = w.Id,
            OrderId = w.OrderId,
            SequenceNumber = w.SequenceNumber,
            GrossWeight = w.GrossWeight,
            TareWeight = w.TareWeight,
            NetWeight = w.NetWeight,
            Unit = w.Unit,
            NetWeightInKg = w.NetWeightInKg,
            CollectionPointName = w.CollectionPointName,
            CollectionDistrict = w.CollectionDistrict,
            CollectionUpazila = w.CollectionUpazila,
            OperatorFullName = w.OperatorFullName,
            ScaleIdentifier = w.ScaleIdentifier,
            ScaleCalibrationStatus = w.ScaleCalibrationStatus,
            Status = w.Status,
            IsFarmerConfirmed = w.IsFarmerConfirmed,
            IsBuyerConfirmed = w.IsBuyerConfirmed,
            IsCorrectedRecord = w.IsCorrectedRecord,
            CorrectionReason = w.CorrectionReason,
            ReceiptPhotoUrl = w.ReceiptPhotoUrl,
            Remarks = w.Remarks,
            RecordedAtUtc = w.RecordedAtUtc
        };
    }
}

public class PaymentService : IPaymentService
{
    private readonly IPaymentRecordRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogger _auditLogger;
    private readonly INotificationService _notificationService;

    public PaymentService(
        IPaymentRecordRepository paymentRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditLogger auditLogger,
        INotificationService notificationService)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _auditLogger = auditLogger;
        _notificationService = notificationService;
    }

    public async Task<Result<PaymentRecordDto>> RecordDirectPaymentAsync(RecordPaymentDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetWithFullDetailsAsync(dto.OrderId, cancellationToken);
        if (order == null)
        {
            return Result<PaymentRecordDto>.Failure("অর্ডার পাওয়া যায়নি।");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            return Result<PaymentRecordDto>.Failure("বাতিল অর্ডারে পেমেন্ট গ্রহণ করা যাবে না।");
        }

        if (dto.Amount <= 0)
        {
            return Result<PaymentRecordDto>.Failure("পেমেন্টের পরিমাণ শূন্যের চেয়ে বেশি হতে হবে।");
        }

        // Check for duplicate transaction reference to prevent double entry
        var existingTrx = await _paymentRepository.FindAsync(p => p.TransactionReference == dto.TransactionReference && p.Method == dto.Method, cancellationToken);
        if (existingTrx.Any())
        {
            return Result<PaymentRecordDto>.Failure("এই ট্রানজ্যাকশন আইডি (TrxID) ইতিপূর্বে ব্যবহার করা হয়েছে। অনুগ্রহ করে সঠিক তথ্য প্রদান করুন।");
        }

        var payment = new PaymentRecord
        {
            OrderId = order.Id,
            Amount = dto.Amount,
            Method = dto.Method,
            TransactionReference = dto.TransactionReference.Trim(),
            PaymentDateUtc = DateTime.UtcNow,
            SenderAccountOrPhone = dto.SenderAccountOrPhone?.Trim(),
            ReceiverAccountOrPhone = dto.ReceiverAccountOrPhone?.Trim(),
            ReceiptEvidenceUrl = dto.ReceiptEvidenceUrl,
            // SECURITY REQUIREMENT: Never auto-mark as verified!
            Status = PaymentStatus.PendingVerification
        };

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync("PaymentRecord", payment.Id.ToString(), "SubmitPayment",
            $"Submitted Payment ৳{payment.Amount} via {payment.Method} (TrxID: {payment.TransactionReference}). Awaiting Verification.", cancellationToken);

        // Notify farmer about incoming payment to verify
        if (order.FarmerProfile != null)
        {
            await _notificationService.SendNotificationAsync(
                order.FarmerProfile.UserId,
                "নতুন পেমেন্ট ট্রানজ্যাকশন জমা পড়েছে",
                "New Payment Submitted",
                $"অর্ডার #{order.OrderNumber}-এ ৳{payment.Amount} ({payment.Method}) পেমেন্ট জমা দেওয়া হয়েছে। TrxID: {payment.TransactionReference}। আপনার অ্যাকাউন্টে টাকা জমা যাচাই করে নিশ্চিত করুন।",
                $"Payment of ৳{payment.Amount} submitted for Order #{order.OrderNumber}. Please verify receipt.",
                "Payment",
                $"/orders/{order.Id}",
                cancellationToken);
        }

        return Result<PaymentRecordDto>.Success(MapToDto(payment), "পেমেন্ট তথ্য সফলভাবে জমা হয়েছে। কৃষক বা দায়িত্বপ্রাপ্ত কর্মকর্তার দ্বারা যাচাইকরণের পর চূড়ান্ত হিসাব সমন্বয় করা হবে।");
    }

    public async Task<Result> VerifyPaymentAsync(VerifyPaymentDto dto, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(dto.PaymentRecordId, cancellationToken);
        if (payment == null) return Result.Failure("পেমেন্ট রেকর্ড পাওয়া যায়নি।");

        var order = await _orderRepository.GetWithFullDetailsAsync(payment.OrderId, cancellationToken);
        if (order == null) return Result.Failure("সম্পর্কিত অর্ডার পাওয়া যায়নি।");

        var currentUserId = _currentUserService.UserId;
        var role = _currentUserService.Role;

        // Verify authorization: Farmer (the receiver) or SupportAgent/Admin
        bool isAuthorized = (role == UserRole.Administrator || role == UserRole.SupportAgent) ||
                            (role == UserRole.Farmer && order.FarmerProfile?.UserId == currentUserId);

        if (!isAuthorized)
        {
            return Result.Failure("শুধুমাত্র টাকা গ্রহণকারী কৃষক বা দায়িত্বপ্রাপ্ত কর্মকর্তা এই পেমেন্ট যাচাই করতে পারেন।");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            if (dto.IsApproved)
            {
                payment.Status = PaymentStatus.Paid;
                payment.VerifiedByUserId = currentUserId;
                payment.VerifiedByRole = role?.ToString();
                payment.VerifiedAtUtc = DateTime.UtcNow;
                payment.VerificationNotes = dto.VerificationNotes;
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.VerifiedByUserId = currentUserId;
                payment.VerifiedByRole = role?.ToString();
                payment.VerifiedAtUtc = DateTime.UtcNow;
                payment.VerificationNotes = dto.VerificationNotes;
            }

            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Recalculate Verified Paid Amount from database
            var totalVerifiedPaid = await _paymentRepository.GetTotalVerifiedAmountPaidForOrderAsync(order.Id, cancellationToken);
            order.VerifiedPaidAmount = totalVerifiedPaid;

            // Recalculate order lifecycle status based on verified balance
            if (order.OutstandingBalance <= 0)
            {
                order.Status = OrderStatus.Completed;
                order.CompletedAtUtc = DateTime.UtcNow;
            }
            else if (order.VerifiedPaidAmount > 0)
            {
                order.Status = OrderStatus.PaymentPartiallyReceived;
            }
            else if (order.Status == OrderStatus.DeliveredPendingConfirmation || order.Status == OrderStatus.AwaitingDelivery)
            {
                order.Status = OrderStatus.PaymentPending;
            }

            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            await _auditLogger.LogAsync("PaymentRecord", payment.Id.ToString(), "VerifyPayment",
                $"Verified payment ৳{payment.Amount}. Result={payment.Status}. Order Total Paid={totalVerifiedPaid}, Balance={order.OutstandingBalance}", cancellationToken);

            return Result.Success(dto.IsApproved
                ? $"পেমেন্ট সফলভাবে যাচাই হয়েছে। অর্ডারের বকেয়া পাওনা এখন: ৳ {order.OutstandingBalance:N2}"
                : "পেমেন্টটি অগ্রাহ্য/প্রত্যাখ্যাত করা হয়েছে।");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result.Failure($"পেমেন্ট যাচাইকরণে ত্রুটি: {ex.Message}");
        }
    }

    public async Task<IReadOnlyList<PaymentRecordDto>> GetPaymentHistoryForOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetPaymentsForOrderAsync(orderId, cancellationToken);
        return payments.Select(MapToDto).ToList();
    }

    private static PaymentRecordDto MapToDto(PaymentRecord p)
    {
        return new PaymentRecordDto
        {
            Id = p.Id,
            OrderId = p.OrderId,
            Amount = p.Amount,
            Method = p.Method,
            TransactionReference = p.TransactionReference,
            PaymentDateUtc = p.PaymentDateUtc,
            SenderAccountOrPhone = p.SenderAccountOrPhone,
            ReceiverAccountOrPhone = p.ReceiverAccountOrPhone,
            ReceiptEvidenceUrl = p.ReceiptEvidenceUrl,
            Status = p.Status,
            VerifiedByRole = p.VerifiedByRole,
            VerifiedAtUtc = p.VerifiedAtUtc,
            VerificationNotes = p.VerificationNotes
        };
    }
}
