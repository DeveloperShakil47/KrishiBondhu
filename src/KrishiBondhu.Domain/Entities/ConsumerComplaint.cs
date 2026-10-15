using System;
using KrishiBondhu.Domain.Common;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Domain.Entities;

public class ConsumerComplaint : BaseEntity
{
    public string TrackingNumber { get; set; } = string.Empty;

    // Complainant Information (Protected private data)
    public string? ComplainantUserId { get; set; }
    public string ComplainantName { get; set; } = string.Empty;
    public string ComplainantPhone { get; set; } = string.Empty;
    public string? ComplainantEmail { get; set; }

    // Reported Seller Information
    public string ShopOrBusinessName { get; set; } = string.Empty;
    public string ShopAddress { get; set; } = string.Empty;
    
    public Guid? MarketId { get; set; }
    public Market? Market { get; set; }

    public string District { get; set; } = string.Empty;
    public string Upazila { get; set; } = string.Empty;

    public MarketType SellerType { get; set; } = MarketType.Retail;

    // Transaction Details
    public Guid CropId { get; set; }
    public Crop? Crop { get; set; }

    public decimal QuantityPurchased { get; set; }
    public string ReportedUnit { get; set; } = "কেজি";

    public decimal ActualPriceCharged { get; set; }

    // Server-calculated Normalized Metrics
    public decimal NormalizedPricePerUnit { get; set; }
    public string StandardUnit { get; set; } = "কেজি";

    public DateTime PurchaseDateTime { get; set; }
    public string? Description { get; set; }

    // Evidence / Receipt (Secure private server-side storage)
    public string? ReceiptEvidencePath { get; set; }
    public string? ReceiptFileName { get; set; }
    public long? ReceiptFileSizeBytes { get; set; }
    public string? ReceiptContentType { get; set; }

    // Status & Review Workflow
    public ComplaintStatus Status { get; set; } = ComplaintStatus.Submitted;
    public string? AssignedReviewerUserId { get; set; }
    public string? ReviewerNotes { get; set; }
    public string? DecisionReason { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }

    // Comparison against Market Observations (Neutral, objective facts)
    public decimal? MatchedMarketAveragePrice { get; set; }
    public decimal? MatchedMarketMinPrice { get; set; }
    public decimal? MatchedMarketMaxPrice { get; set; }
    public decimal? PriceVariancePercentage { get; set; }

    // Comparison against Official Directives (if an applicable legal directive exists)
    public Guid? OfficialPriceDirectiveId { get; set; }
    public OfficialPriceDirective? OfficialPriceDirective { get; set; }
    public decimal? OfficialLegalPrice { get; set; }
    public bool IsOfficialPriceExceeded { get; set; }

    // Neutral analytical summary generated server-side
    public string? ComparisonAnalysisNotes { get; set; }

    // Publication status (Strictly non-public by default; requires explicit review)
    public bool IsPubliclyVisible { get; set; } = false;
    public string? PublicSummary { get; set; }

    // Security & Spam Prevention
    public string? ClientIpHash { get; set; }
    public string? UserAgent { get; set; }
}
