using KrishiBondhu.Domain.Common;

namespace KrishiBondhu.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // Create, Update, Delete, Transition, VerifyPayment, CorrectWeight, ResolveDispute
    public Guid? PerformedByUserId { get; set; }
    public string? PerformedByUsername { get; set; }
    public string? PerformedByRole { get; set; }
    public string? DetailsJson { get; set; }
    public string? IpAddress { get; set; }
}

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public string TitleBn { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string MessageBn { get; set; } = string.Empty;
    public string MessageEn { get; set; } = string.Empty;
    public string NotificationType { get; set; } = "Info"; // "Offer", "Order", "Weighing", "Payment", "Dispute", "System"
    public string? TargetUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAtUtc { get; set; }
}
