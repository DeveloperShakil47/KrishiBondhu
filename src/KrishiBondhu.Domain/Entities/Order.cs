using KrishiBondhu.Domain.Common;
using KrishiBondhu.Domain.Enums;
using KrishiBondhu.Domain.Exceptions;

namespace KrishiBondhu.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty; // e.g., "KB-2026-00012"
    public string IdempotencyKey { get; set; } = string.Empty;

    public Guid HarvestLotId { get; set; }
    public HarvestLot? HarvestLot { get; set; }

    public Guid BuyerOfferId { get; set; }
    public BuyerOffer? BuyerOffer { get; set; }

    public Guid FarmerProfileId { get; set; }
    public FarmerProfile? FarmerProfile { get; set; }

    public Guid BuyerProfileId { get; set; }
    public BuyerProfile? BuyerProfile { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.OfferAccepted;

    // Snapshot of agreed terms at acceptance time (immutable)
    public decimal AgreedPricePerKg { get; set; }
    public decimal AgreedQuantityKg { get; set; }
    public QualityGrade AgreedGrade { get; set; }
    public string AgreedDeliveryTerms { get; set; } = string.Empty;
    public string AgreedPaymentTerms { get; set; } = string.Empty;
    public decimal AgreedTransportDeduction { get; set; }
    public decimal AgreedWeighingDeduction { get; set; }
    public decimal AgreedPackagingDeduction { get; set; }
    public decimal AgreedPlatformFee { get; set; }
    public decimal AgreedOtherDeductions { get; set; }

    // Final verified quantities and figures (updated upon weighing and verification)
    public decimal? VerifiedNetQuantityKg { get; set; }

    /// <summary>
    /// Effective quantity used for billing: VerifiedNetQuantityKg if recorded & confirmed, else AgreedQuantityKg
    /// </summary>
    public decimal EffectiveQuantityKg => VerifiedNetQuantityKg ?? AgreedQuantityKg;

    public decimal TotalGrossAmount => Math.Round(EffectiveQuantityKg * AgreedPricePerKg, 2, MidpointRounding.AwayFromZero);

    public decimal TotalAgreedDeductions =>
        Math.Round(AgreedTransportDeduction + AgreedWeighingDeduction + AgreedPackagingDeduction + AgreedPlatformFee + AgreedOtherDeductions, 2, MidpointRounding.AwayFromZero);

    public decimal NetPayableToFarmer => Math.Round(Math.Max(0, TotalGrossAmount - TotalAgreedDeductions), 2, MidpointRounding.AwayFromZero);

    // Payments summary (server-verified calculation)
    public decimal VerifiedPaidAmount { get; set; }

    public decimal OutstandingBalance => Math.Round(Math.Max(0, NetPayableToFarmer - VerifiedPaidAmount), 2, MidpointRounding.AwayFromZero);

    public string TermsSnapshotJson { get; set; } = "{}";

    public DateTime AcceptedAtUtc { get; set; } = DateTime.UtcNow;
    public Guid AcceptedByUserId { get; set; }

    public DateTime? DeliveredAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public string? CancellationReason { get; set; }

    // Navigation
    public ICollection<WeighingRecord> WeighingRecords { get; set; } = new List<WeighingRecord>();
    public ICollection<PaymentRecord> PaymentRecords { get; set; } = new List<PaymentRecord>();
    public ICollection<DeliveryRecord> DeliveryRecords { get; set; } = new List<DeliveryRecord>();
    public ICollection<DisputeCase> Disputes { get; set; } = new List<DisputeCase>();

    // Lifecycle state transition method with strict validation
    public void TransitionTo(OrderStatus newStatus, string? reason = null)
    {
        if (Status == OrderStatus.Completed && newStatus != OrderStatus.Completed)
        {
            throw new InvalidOrderStateTransitionException(Status.ToString(), newStatus.ToString(), "Completed orders cannot be transitioned to another state.");
        }

        if (Status == OrderStatus.Cancelled && newStatus != OrderStatus.Cancelled)
        {
            throw new InvalidOrderStateTransitionException(Status.ToString(), newStatus.ToString(), "Cancelled orders cannot be reopened.");
        }

        // Validate allowed transitions
        bool isAllowed = (Status, newStatus) switch
        {
            (OrderStatus.Draft, OrderStatus.OfferAccepted) => true,
            (OrderStatus.OfferAccepted, OrderStatus.AwaitingDelivery) => true,
            (OrderStatus.OfferAccepted, OrderStatus.Cancelled) => true,
            (OrderStatus.AwaitingDelivery, OrderStatus.DeliveredPendingConfirmation) => true,
            (OrderStatus.AwaitingDelivery, OrderStatus.Disputed) => true,
            (OrderStatus.AwaitingDelivery, OrderStatus.Cancelled) => true,
            (OrderStatus.DeliveredPendingConfirmation, OrderStatus.PaymentPending) => true,
            (OrderStatus.DeliveredPendingConfirmation, OrderStatus.Disputed) => true,
            (OrderStatus.PaymentPending, OrderStatus.PaymentPartiallyReceived) => true,
            (OrderStatus.PaymentPending, OrderStatus.Completed) => OutstandingBalance <= 0,
            (OrderStatus.PaymentPending, OrderStatus.Disputed) => true,
            (OrderStatus.PaymentPartiallyReceived, OrderStatus.Completed) => OutstandingBalance <= 0,
            (OrderStatus.PaymentPartiallyReceived, OrderStatus.Disputed) => true,
            (OrderStatus.Disputed, OrderStatus.AwaitingDelivery) => true,
            (OrderStatus.Disputed, OrderStatus.DeliveredPendingConfirmation) => true,
            (OrderStatus.Disputed, OrderStatus.PaymentPending) => true,
            (OrderStatus.Disputed, OrderStatus.Completed) => OutstandingBalance <= 0,
            (OrderStatus.Disputed, OrderStatus.Cancelled) => true,
            _ => false
        };

        if (!isAllowed)
        {
            throw new InvalidOrderStateTransitionException(Status.ToString(), newStatus.ToString(), $"Direct transition from {Status} to {newStatus} is not permitted by business rules.");
        }

        Status = newStatus;
        if (newStatus == OrderStatus.Completed)
        {
            CompletedAtUtc = DateTime.UtcNow;
        }
        else if (newStatus == OrderStatus.Cancelled)
        {
            CancelledAtUtc = DateTime.UtcNow;
            CancellationReason = reason;
        }
    }
}
