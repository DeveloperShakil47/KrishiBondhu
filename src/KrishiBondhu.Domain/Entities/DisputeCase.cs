using KrishiBondhu.Domain.Common;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Domain.Entities;

public class DisputeCase : BaseEntity
{
    public string DisputeNumber { get; set; } = string.Empty; // e.g. "DSP-2026-0042"

    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public Guid RaisedByUserId { get; set; }
    public string RaisedByRole { get; set; } = string.Empty; // "Farmer" or "Buyer"
    public string RaisedByName { get; set; } = string.Empty;

    public DisputeReason Reason { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal? DisputedAmount { get; set; }
    public decimal? DisputedQuantityKg { get; set; }

    public DisputeStatus Status { get; set; } = DisputeStatus.Open;

    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToRole { get; set; }
    public string? AssignedToName { get; set; }

    public string? ResolutionNotes { get; set; }
    public decimal? FinalAdjustedAmount { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public Guid? ResolvedByUserId { get; set; }
}
