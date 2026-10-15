using KrishiBondhu.Domain.Common;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Domain.Entities;

public class PaymentRecord : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string TransactionReference { get; set; } = string.Empty; // TrxID or Bank Slip / Voucher No
    public DateTime PaymentDateUtc { get; set; } = DateTime.UtcNow;

    public string? SenderAccountOrPhone { get; set; }
    public string? ReceiverAccountOrPhone { get; set; }
    public string? ReceiptEvidenceUrl { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.PendingVerification;

    // Manual Verification Audit
    public Guid? VerifiedByUserId { get; set; }
    public string? VerifiedByRole { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }
    public string? VerificationNotes { get; set; }

    public bool IsRefund { get; set; }
    public string? RefundReason { get; set; }
}
