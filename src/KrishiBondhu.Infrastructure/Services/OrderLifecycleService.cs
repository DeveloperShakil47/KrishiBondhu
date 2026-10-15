using System;
using System.Linq;
using System.Text.Json;
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

public class OrderLifecycleService : IOrderLifecycleService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IBuyerOfferRepository _offerRepository;
    private readonly IHarvestLotRepository _lotRepository;
    private readonly IFarmerProfileRepository _farmerRepository;
    private readonly IBuyerProfileRepository _buyerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogger _auditLogger;
    private readonly INotificationService _notificationService;

    public OrderLifecycleService(
        IOrderRepository orderRepository,
        IBuyerOfferRepository offerRepository,
        IHarvestLotRepository lotRepository,
        IFarmerProfileRepository farmerRepository,
        IBuyerProfileRepository buyerRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditLogger auditLogger,
        INotificationService notificationService)
    {
        _orderRepository = orderRepository;
        _offerRepository = offerRepository;
        _lotRepository = lotRepository;
        _farmerRepository = farmerRepository;
        _buyerRepository = buyerRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _auditLogger = auditLogger;
        _notificationService = notificationService;
    }

    public async Task<Result<OrderDto>> AcceptOfferAndCreateOrderAsync(AcceptOfferDto dto, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (!currentUserId.HasValue)
        {
            return Result<OrderDto>.Failure("অননুমোদিত ব্যবহারকারী। লগইন করুন।");
        }

        // Idempotency check: prevent duplicate order creation from double-clicks
        var existingOrder = await _orderRepository.GetByIdempotencyKeyAsync(dto.IdempotencyKey, cancellationToken);
        if (existingOrder != null)
        {
            var existingFull = await _orderRepository.GetWithFullDetailsAsync(existingOrder.Id, cancellationToken);
            return Result<OrderDto>.Success(MapToDto(existingFull!), "এই দরপ্রস্তাবের অর্ডারটি ইতিমধ্যে সফলভাবে তৈরি হয়েছে।");
        }

        var offer = await _offerRepository.GetWithDetailsAsync(dto.BuyerOfferId, cancellationToken);
        if (offer == null)
        {
            return Result<OrderDto>.Failure("দরপ্রস্তাবটি পাওয়া যায়নি।");
        }

        if (offer.ExpiryDateUtc <= DateTime.UtcNow)
        {
            offer.Status = OfferStatus.Expired;
            await _offerRepository.UpdateAsync(offer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<OrderDto>.Failure("এই দরপ্রস্তাবের মেয়াদ শেষ হয়ে গেছে।");
        }

        if (offer.Status != OfferStatus.Submitted && offer.Status != OfferStatus.UnderReview)
        {
            return Result<OrderDto>.Failure($"এই দরপ্রস্তাবটি গ্রহণ করা সম্ভব নয় (বর্তমান অবস্থা: {offer.Status})।");
        }

        var lot = await _lotRepository.GetWithDetailsAsync(offer.HarvestLotId, cancellationToken);
        if (lot == null)
        {
            return Result<OrderDto>.Failure("সম্পর্কিত ফসলের লট পাওয়া যায়নি।");
        }

        // Authorization check: only the farmer who owns the lot can accept the offer
        var farmer = await _farmerRepository.GetByUserIdAsync(currentUserId.Value, cancellationToken);
        if (farmer == null || lot.FarmerProfileId != farmer.Id)
        {
            return Result<OrderDto>.Failure("শুধুমাত্র ফসলের মালিক কৃষক এই দরপ্রস্তাব গ্রহণ করতে পারেন।");
        }

        var buyer = await _buyerRepository.GetByIdAsync(offer.BuyerProfileId, cancellationToken);
        if (buyer == null)
        {
            return Result<OrderDto>.Failure("ক্রেতার তথ্য পাওয়া যায়নি।");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var orderNumber = await _orderRepository.GenerateNextOrderNumberAsync(cancellationToken);

            // Construct immutable snapshot of agreed terms
            var termsSnapshot = new
            {
                OrderNumber = orderNumber,
                OfferCode = offer.OfferCode,
                CropName = lot.Crop?.NameBn,
                LotNumber = lot.LotNumber,
                AgreedPricePerKg = offer.OfferedPricePerKg,
                AgreedQuantityKg = offer.OfferedQuantityKg,
                Grade = lot.Grade.ToString(),
                DeliveryTerms = offer.DeliveryTerms,
                PaymentTerms = offer.PaymentTerms,
                Deductions = new
                {
                    Transport = offer.TransportCostDeduction,
                    WeighingLabor = offer.WeighingAndLaborCostDeduction,
                    Packaging = offer.PackagingCostDeduction,
                    PlatformFee = offer.PlatformFee,
                    Other = offer.OtherDeductions,
                    Total = offer.TotalEstimatedDeductions
                },
                GrossSaleValue = offer.GrossSaleValue,
                EstimatedNetProceeds = offer.EstimatedNetProceeds,
                AcceptedAtUtc = DateTime.UtcNow,
                AcceptedByUserId = currentUserId.Value
            };

            var order = new Order
            {
                OrderNumber = orderNumber,
                IdempotencyKey = dto.IdempotencyKey,
                HarvestLotId = lot.Id,
                BuyerOfferId = offer.Id,
                FarmerProfileId = farmer.Id,
                BuyerProfileId = buyer.Id,
                Status = OrderStatus.OfferAccepted,
                AgreedPricePerKg = offer.OfferedPricePerKg,
                AgreedQuantityKg = offer.OfferedQuantityKg,
                AgreedGrade = lot.Grade,
                AgreedDeliveryTerms = offer.DeliveryTerms,
                AgreedPaymentTerms = offer.PaymentTerms,
                AgreedTransportDeduction = offer.TransportCostDeduction,
                AgreedWeighingDeduction = offer.WeighingAndLaborCostDeduction,
                AgreedPackagingDeduction = offer.PackagingCostDeduction,
                AgreedPlatformFee = offer.PlatformFee,
                AgreedOtherDeductions = offer.OtherDeductions,
                VerifiedNetQuantityKg = null,
                VerifiedPaidAmount = 0,
                TermsSnapshotJson = JsonSerializer.Serialize(termsSnapshot),
                AcceptedAtUtc = DateTime.UtcNow,
                AcceptedByUserId = currentUserId.Value
            };

            await _orderRepository.AddAsync(order, cancellationToken);

            // Update Offer Status
            offer.Status = OfferStatus.Accepted;
            offer.RespondedAtUtc = DateTime.UtcNow;
            await _offerRepository.UpdateAsync(offer, cancellationToken);

            // Update Lot Status
            lot.Status = LotStatus.Sold;
            await _lotRepository.UpdateAsync(lot, cancellationToken);

            // Reject all other pending offers for this lot
            await _offerRepository.RejectOtherOffersForLotAsync(lot.Id, offer.Id, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            await _auditLogger.LogAsync("Order", order.Id.ToString(), "CreateOrderFromOffer", $"Order {orderNumber} created from Offer {offer.OfferCode}", cancellationToken);

            // Notify buyer
            await _notificationService.SendNotificationAsync(
                buyer.UserId,
                "দরপ্রস্তাব গৃহীত হয়েছে!",
                "Offer Accepted!",
                $"কৃষক {farmer.FullNameBn} আপনার {offer.OfferCode} দরপ্রস্তাবটি গ্রহণ করেছেন। অর্ডার নম্বর: {orderNumber}।",
                $"Farmer {farmer.FullNameBn} accepted your offer {offer.OfferCode}. Order Number: {orderNumber}.",
                "Order",
                $"/orders/{order.Id}",
                cancellationToken);

            var createdFull = await _orderRepository.GetWithFullDetailsAsync(order.Id, cancellationToken);
            return Result<OrderDto>.Success(MapToDto(createdFull!), "দরপ্রস্তাব গৃহীত হয়েছে এবং চুক্তিভিত্তিক অর্ডার তৈরি হয়েছে।");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<OrderDto>.Failure($"অর্ডার তৈরিতে ত্রুটি: {ex.Message}");
        }
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetWithFullDetailsAsync(orderId, cancellationToken);
        if (order == null) return null;
        return MapToDto(order);
    }

    public async Task<PaginatedList<OrderDto>> GetMyOrdersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        var role = _currentUserService.Role;
        if (!userId.HasValue)
        {
            return new PaginatedList<OrderDto>(Array.Empty<OrderDto>(), 0, pageNumber, pageSize);
        }

        Guid? farmerProfileId = null;
        Guid? buyerProfileId = null;

        if (role == UserRole.Farmer)
        {
            var farmer = await _farmerRepository.GetByUserIdAsync(userId.Value, cancellationToken);
            if (farmer != null) farmerProfileId = farmer.Id;
        }
        else if (role == UserRole.Buyer)
        {
            var buyer = await _buyerRepository.GetByUserIdAsync(userId.Value, cancellationToken);
            if (buyer != null) buyerProfileId = buyer.Id;
        }

        var paginated = await _orderRepository.SearchOrdersAsync(null, null, farmerProfileId, buyerProfileId, pageNumber, pageSize, cancellationToken);

        var dtos = paginated.Items.Select(MapToDto).ToList();
        return new PaginatedList<OrderDto>(dtos, paginated.TotalCount, paginated.PageNumber, pageSize);
    }

    public async Task<Result> TransitionOrderStatusAsync(Guid orderId, OrderStatus newStatus, string? reason = null, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetWithFullDetailsAsync(orderId, cancellationToken);
        if (order == null) return Result.Failure("অর্ডার পাওয়া যায়নি।");

        try
        {
            order.TransitionTo(newStatus, reason);
            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditLogger.LogAsync("Order", order.Id.ToString(), "TransitionStatus", $"Transitioned to {newStatus}. Reason: {reason}", cancellationToken);

            return Result.Success($"অর্ডারের অবস্থা পরিবর্তিত হয়েছে: {newStatus}");
        }
        catch (InvalidOrderStateTransitionException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private static OrderDto MapToDto(Order o)
    {
        return new OrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            HarvestLotId = o.HarvestLotId,
            LotNumber = o.HarvestLot?.LotNumber ?? "",
            CropNameBn = o.HarvestLot?.Crop?.NameBn ?? "",
            FarmerProfileId = o.FarmerProfileId,
            FarmerNameBn = o.FarmerProfile?.FullNameBn ?? "",
            FarmerPhoneNumber = o.FarmerProfile?.PhoneNumber ?? "",
            BuyerProfileId = o.BuyerProfileId,
            BuyerBusinessName = o.BuyerProfile?.BusinessName ?? "",
            BuyerPhoneNumber = o.BuyerProfile?.PhoneNumber ?? "",
            Status = o.Status,
            AgreedPricePerKg = o.AgreedPricePerKg,
            AgreedQuantityKg = o.AgreedQuantityKg,
            AgreedGrade = o.AgreedGrade,
            AgreedDeliveryTerms = o.AgreedDeliveryTerms,
            AgreedPaymentTerms = o.AgreedPaymentTerms,
            AgreedTransportDeduction = o.AgreedTransportDeduction,
            AgreedWeighingDeduction = o.AgreedWeighingDeduction,
            AgreedPackagingDeduction = o.AgreedPackagingDeduction,
            AgreedPlatformFee = o.AgreedPlatformFee,
            AgreedOtherDeductions = o.AgreedOtherDeductions,
            VerifiedNetQuantityKg = o.VerifiedNetQuantityKg,
            EffectiveQuantityKg = o.EffectiveQuantityKg,
            TotalGrossAmount = o.TotalGrossAmount,
            TotalAgreedDeductions = o.TotalAgreedDeductions,
            NetPayableToFarmer = o.NetPayableToFarmer,
            VerifiedPaidAmount = o.VerifiedPaidAmount,
            OutstandingBalance = o.OutstandingBalance,
            AcceptedAtUtc = o.AcceptedAtUtc,
            DeliveredAtUtc = o.DeliveredAtUtc,
            CompletedAtUtc = o.CompletedAtUtc,
            CancelledAtUtc = o.CancelledAtUtc,
            CancellationReason = o.CancellationReason,
            WeighingRecords = o.WeighingRecords.Select(w => new WeighingRecordDto
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
            }).ToList(),
            PaymentRecords = o.PaymentRecords.Select(p => new PaymentRecordDto
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
            }).ToList(),
            Disputes = o.Disputes.Select(d => new DisputeCaseDto
            {
                Id = d.Id,
                DisputeNumber = d.DisputeNumber,
                OrderId = d.OrderId,
                OrderNumber = o.OrderNumber,
                RaisedByName = d.RaisedByName,
                RaisedByRole = d.RaisedByRole,
                Reason = d.Reason,
                Description = d.Description,
                DisputedAmount = d.DisputedAmount,
                DisputedQuantityKg = d.DisputedQuantityKg,
                Status = d.Status,
                AssignedToName = d.AssignedToName,
                ResolutionNotes = d.ResolutionNotes,
                FinalAdjustedAmount = d.FinalAdjustedAmount,
                CreatedAtUtc = d.CreatedAtUtc,
                ResolvedAtUtc = d.ResolvedAtUtc
            }).ToList()
        };
    }
}
