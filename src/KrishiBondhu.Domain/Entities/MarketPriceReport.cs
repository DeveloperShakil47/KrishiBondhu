using System;
using System.Collections.Generic;
using KrishiBondhu.Domain.Common;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Domain.Entities;

public class MarketPriceReport : BaseEntity
{
    public Guid MarketId { get; set; }
    public Market? Market { get; set; }

    public DateTime ReportDate { get; set; }

    public Guid PriceSourceId { get; set; }
    public PriceSource? PriceSource { get; set; }

    public string? SubmittedByUserId { get; set; }

    public PriceVerificationStatus VerificationStatus { get; set; } = PriceVerificationStatus.Unverified;
    public string? VerifiedByUserId { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }
    public string? VerificationNotes { get; set; }

    public string? Notes { get; set; }

    // Navigation
    public ICollection<MarketPriceObservation> Observations { get; set; } = new List<MarketPriceObservation>();
}
