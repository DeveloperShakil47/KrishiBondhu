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
using KrishiBondhu.Infrastructure.Data;

namespace KrishiBondhu.Infrastructure.Services;

public class DisputeService : IDisputeService
{
    private readonly IDisputeCaseRepository _disputeRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogger _auditLogger;
    private readonly INotificationService _notificationService;

    public DisputeService(
        IDisputeCaseRepository disputeRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditLogger auditLogger,
        INotificationService notificationService)
    {
        _disputeRepository = disputeRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _auditLogger = auditLogger;
        _notificationService = notificationService;
    }

    public async Task<Result<DisputeCaseDto>> RaiseDisputeAsync(RaiseDisputeDto dto, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (!currentUserId.HasValue)
        {
            return Result<DisputeCaseDto>.Failure("অননুমোদিত ব্যবহারকারী।");
        }

        var order = await _orderRepository.GetWithFullDetailsAsync(dto.OrderId, cancellationToken);
        if (order == null)
        {
            return Result<DisputeCaseDto>.Failure("অর্ডার পাওয়া যায়নি।");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            return Result<DisputeCaseDto>.Failure("বাতিল অর্ডারে বিরোধ তৈরি করা যাবে না।");
        }

        var disputeNumber = await _disputeRepository.GenerateNextDisputeNumberAsync(cancellationToken);

        var dispute = new DisputeCase
        {
            DisputeNumber = disputeNumber,
            OrderId = order.Id,
            RaisedByUserId = currentUserId.Value,
            RaisedByRole = _currentUserService.Role?.ToString() ?? "User",
            RaisedByName = _currentUserService.FullName ?? _currentUserService.Username ?? "Unknown",
            Reason = dto.Reason,
            Description = dto.Description.Trim(),
            DisputedAmount = dto.DisputedAmount,
            DisputedQuantityKg = dto.DisputedQuantityKg,
            Status = DisputeStatus.Open
        };

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _disputeRepository.AddAsync(dispute, cancellationToken);

            // Put order into Disputed status so it cannot be completed while contested
            order.Status = OrderStatus.Disputed;
            await _orderRepository.UpdateAsync(order, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            await _auditLogger.LogAsync("DisputeCase", dispute.Id.ToString(), "RaiseDispute",
                $"Raised Dispute {disputeNumber} for Order {order.OrderNumber}. Reason: {dispute.Reason}", cancellationToken);

            return Result<DisputeCaseDto>.Success(MapToDto(dispute, order.OrderNumber), "বিরোধ সফলভাবে নথিভুক্ত হয়েছে। একজন সহায়তা কর্মকর্তা এটি পর্যালোচনা করবেন।");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<DisputeCaseDto>.Failure($"বিরোধ নথিবদ্ধকরণে ত্রুটি: {ex.Message}");
        }
    }

    public async Task<Result> ResolveDisputeAsync(ResolveDisputeDto dto, CancellationToken cancellationToken = default)
    {
        var role = _currentUserService.Role;
        if (role != UserRole.Administrator && role != UserRole.SupportAgent)
        {
            return Result.Failure("শুধুমাত্র সহায়তা কর্মকর্তা বা প্রশাসক বিরোধ নিষ্পত্তি করতে পারেন।");
        }

        var dispute = await _disputeRepository.GetWithDetailsAsync(dto.DisputeId, cancellationToken);
        if (dispute == null) return Result.Failure("বিরোধের রেকর্ড পাওয়া যায়নি।");

        var order = await _orderRepository.GetWithFullDetailsAsync(dispute.OrderId, cancellationToken);
        if (order == null) return Result.Failure("সম্পর্কিত অর্ডার পাওয়া যায়নি।");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            dispute.Status = dto.IsResolved ? DisputeStatus.Resolved : DisputeStatus.Rejected;
            dispute.ResolutionNotes = dto.ResolutionNotes;
            dispute.FinalAdjustedAmount = dto.FinalAdjustedAmount;
            dispute.ResolvedAtUtc = DateTime.UtcNow;
            dispute.ResolvedByUserId = _currentUserService.UserId;
            dispute.AssignedToName = _currentUserService.FullName ?? _currentUserService.Username;

            await _disputeRepository.UpdateAsync(dispute, cancellationToken);

            // If adjustments are applied, update deductions or figures on order
            if (dto.FinalAdjustedAmount.HasValue && dto.FinalAdjustedAmount.Value != 0)
            {
                order.AgreedOtherDeductions += dto.FinalAdjustedAmount.Value;
            }

            // Return order to appropriate active status (e.g. PaymentPending)
            if (order.OutstandingBalance <= 0)
            {
                order.Status = OrderStatus.Completed;
                order.CompletedAtUtc = DateTime.UtcNow;
            }
            else
            {
                order.Status = OrderStatus.PaymentPending;
            }

            await _orderRepository.UpdateAsync(order, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            await _auditLogger.LogAsync("DisputeCase", dispute.Id.ToString(), "ResolveDispute",
                $"Resolved Dispute {dispute.DisputeNumber}. Decision: {dispute.Status}. Adjustment: {dto.FinalAdjustedAmount}", cancellationToken);

            // Notify both parties
            if (order.FarmerProfile != null)
            {
                await _notificationService.SendNotificationAsync(
                    order.FarmerProfile.UserId,
                    "বিরোধ মীমাংসা সম্পন্ন হয়েছে",
                    "Dispute Resolved",
                    $"অর্ডার #{order.OrderNumber}-এর বিরোধ #{dispute.DisputeNumber} মীমাংসা করা হয়েছে। সিদ্ধান্ত: {dispute.Status}। নোট: {dispute.ResolutionNotes}",
                    $"Dispute #{dispute.DisputeNumber} resolved.",
                    "Dispute",
                    $"/orders/{order.Id}",
                    cancellationToken);
            }

            return Result.Success("বিরোধের নিষ্পত্তি সফলভাবে সম্পন্ন হয়েছে।");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result.Failure($"মীমাংসায় ত্রুটি: {ex.Message}");
        }
    }

    public async Task<PaginatedList<DisputeCaseDto>> GetDisputesAsync(DisputeStatus? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var paginated = await _disputeRepository.GetDisputesAsync(status, pageNumber, pageSize, cancellationToken);
        var dtos = paginated.Items.Select(d => MapToDto(d, d.Order?.OrderNumber ?? "")).ToList();
        return new PaginatedList<DisputeCaseDto>(dtos, paginated.TotalCount, paginated.PageNumber, pageSize);
    }

    public async Task<DisputeCaseDto?> GetDisputeByIdAsync(Guid disputeId, CancellationToken cancellationToken = default)
    {
        var d = await _disputeRepository.GetWithDetailsAsync(disputeId, cancellationToken);
        if (d == null) return null;
        return MapToDto(d, d.Order?.OrderNumber ?? "");
    }

    private static DisputeCaseDto MapToDto(DisputeCase d, string orderNumber)
    {
        return new DisputeCaseDto
        {
            Id = d.Id,
            DisputeNumber = d.DisputeNumber,
            OrderId = d.OrderId,
            OrderNumber = orderNumber,
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
        };
    }
}

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public NotificationService(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetMyUnreadNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue) return Array.Empty<NotificationDto>();

        var notifications = await _notificationRepository.GetUnreadNotificationsForUserAsync(userId.Value, cancellationToken);
        return notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            TitleBn = n.TitleBn,
            TitleEn = n.TitleEn,
            MessageBn = n.MessageBn,
            MessageEn = n.MessageEn,
            NotificationType = n.NotificationType,
            TargetUrl = n.TargetUrl,
            IsRead = n.IsRead,
            CreatedAtUtc = n.CreatedAtUtc
        }).ToList();
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        await _notificationRepository.MarkAsReadAsync(notificationId, cancellationToken);
    }

    public async Task SendNotificationAsync(
        Guid userId,
        string titleBn,
        string titleEn,
        string messageBn,
        string messageEn,
        string type,
        string? url = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            TitleBn = titleBn,
            TitleEn = titleEn,
            MessageBn = messageBn,
            MessageEn = messageEn,
            NotificationType = type,
            TargetUrl = url,
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
