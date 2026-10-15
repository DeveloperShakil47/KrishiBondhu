using System;
using KrishiBondhu.Domain.Common;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Domain.Entities;

public class MarketPriceObservation : BaseEntity
{
    public Guid CropId { get; set; }
    public Crop? Crop { get; set; }

    public Guid MarketId { get; set; }
    public Market? Market { get; set; }

    public Guid? MarketPriceReportId { get; set; }
    public MarketPriceReport? MarketPriceReport { get; set; }

    public MarketType MarketType { get; set; } = MarketType.Wholesale;

    public DateTime ObservationDate { get; set; }

    public Guid PriceSourceId { get; set; }
    public PriceSource? PriceSource { get; set; }

    // Original observation data
    public decimal ObservedPrice { get; set; }
    public decimal ObservedQuantity { get; set; } = 1.0m;
    public string ObservedUnit { get; set; } = "কেজি";

    // Server-normalized standardized unit and price (BDT per standardized base unit e.g. BDT/Kg, BDT/Litre, BDT/Piece)
    public decimal NormalizedPricePerUnit { get; set; }
    public string NormalizedUnit { get; set; } = "কেজি";

    // Price range observed in the market session
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    // Normalized Min and Max prices
    public decimal? NormalizedMinPrice { get; set; }
    public decimal? NormalizedMaxPrice { get; set; }

    public QualityGrade Grade { get; set; } = QualityGrade.Standard;

    public PriceVerificationStatus VerificationStatus { get; set; } = PriceVerificationStatus.Unverified;
    public string? VerifiedByUserId { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }

    public string? Notes { get; set; }

    // Currency is always BDT
    public string Currency { get; set; } = "BDT";
}
