using System;
using System.ComponentModel.DataAnnotations;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Application.DTOs;

public class SubmitConsumerComplaintDto
{
    // Complainant Information (Confidential)
    [Required(ErrorMessage = "অভিযোগকারীর নাম আবশ্যক")]
    [MaxLength(100, ErrorMessage = "নাম ১০০ অক্ষরের মধ্যে হতে হবে")]
    public string ComplainantName { get; set; } = string.Empty;

    [Required(ErrorMessage = "মোবাইল নম্বর আবশ্যক")]
    [RegularExpression(@"^(?:\+88|88)?(01[3-9]\d{8})$", ErrorMessage = "সঠিক বাংলাদেশী মোবাইল নম্বর প্রদান করুন (যেমন: 017XXXXXXXX)")]
    public string ComplainantPhone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "সঠিক ইমেইল এড্রেস লিখুন")]
    public string? ComplainantEmail { get; set; }

    // Reported Seller Information
    [Required(ErrorMessage = "দোকান বা ব্যবসা প্রতিষ্ঠানের নাম আবশ্যক")]
    [MaxLength(150, ErrorMessage = "দোকানের নাম ১৫০ অক্ষরের মধ্যে হতে হবে")]
    public string ShopOrBusinessName { get; set; } = string.Empty;

    [Required(ErrorMessage = "দোকানের ঠিকানা বা বাজারের নাম আবশ্যক")]
    [MaxLength(250, ErrorMessage = "ঠিকানা ২৫০ অক্ষরের মধ্যে হতে হবে")]
    public string ShopAddress { get; set; } = string.Empty;

    public Guid? MarketId { get; set; }

    [Required(ErrorMessage = "জেলা নির্বাচন করুন")]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "উপজেলা নির্বাচন করুন")]
    public string Upazila { get; set; } = string.Empty;

    [Required(ErrorMessage = "বিক্রেতার ধরন নির্বাচন করুন")]
    public MarketType SellerType { get; set; } = MarketType.Retail;

    // Transaction Details
    [Required(ErrorMessage = "পণ্য বা ফসল নির্বাচন করুন")]
    public Guid CropId { get; set; }

    [Range(0.01, 100000, ErrorMessage = "ক্রয়কৃত পরিমাণ ০ এর বেশি হতে হবে")]
    public decimal QuantityPurchased { get; set; } = 1.0m;

    [Required(ErrorMessage = "একক নির্বাচন করুন (কেজি, লিটার ইত্যাদি)")]
    public string ReportedUnit { get; set; } = "কেজি";

    [Range(0.01, 10000000, ErrorMessage = "প্রকৃত মূল্য ০ এর বেশি হতে হবে")]
    public decimal ActualPriceCharged { get; set; }

    [Required(ErrorMessage = "ক্রয়ের তারিখ ও সময় আবশ্যক")]
    public DateTime PurchaseDateTime { get; set; } = DateTime.Now;

    [MaxLength(1000, ErrorMessage = "বিবরণ ১০০০ অক্ষরের মধ্যে হতে হবে")]
    public string? Description { get; set; }

    // Uploaded Evidence Metadata (File stream processed on server)
    public string? ReceiptEvidencePath { get; set; }
    public string? ReceiptFileName { get; set; }
    public long? ReceiptFileSizeBytes { get; set; }
    public string? ReceiptContentType { get; set; }
}

public class ConsumerComplaintDto
{
    public Guid Id { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;

    // Private submitter info
    public string? ComplainantUserId { get; set; }
    public string ComplainantName { get; set; } = string.Empty;
    public string ComplainantPhone { get; set; } = string.Empty;
    public string? ComplainantEmail { get; set; }

    // Seller info
    public string ShopOrBusinessName { get; set; } = string.Empty;
    public string ShopAddress { get; set; } = string.Empty;
    public Guid? MarketId { get; set; }
    public string? MarketNameBn { get; set; }
    public string District { get; set; } = string.Empty;
    public string Upazila { get; set; } = string.Empty;
    public MarketType SellerType { get; set; }

    // Product info
    public Guid CropId { get; set; }
    public string CropNameBn { get; set; } = string.Empty;
    public decimal QuantityPurchased { get; set; }
    public string ReportedUnit { get; set; } = "কেজি";
    public decimal ActualPriceCharged { get; set; }

    // Server-calculated Normalized Metrics
    public decimal NormalizedPricePerUnit { get; set; }
    public string StandardUnit { get; set; } = "কেজি";

    public DateTime PurchaseDateTime { get; set; }
    public string? Description { get; set; }

    // Evidence
    public bool HasReceiptEvidence { get; set; }
    public string? ReceiptEvidencePath { get; set; }
    public string? ReceiptFileName { get; set; }

    // Status & Review Workflow
    public ComplaintStatus Status { get; set; }
    public string? AssignedReviewerUserId { get; set; }
    public string? ReviewerNotes { get; set; }
    public string? DecisionReason { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }

    // Neutral Price Comparison
    public decimal? MatchedMarketAveragePrice { get; set; }
    public decimal? MatchedMarketMinPrice { get; set; }
    public decimal? MatchedMarketMaxPrice { get; set; }
    public decimal? PriceVariancePercentage { get; set; }

    // Official Directive Comparison
    public Guid? OfficialPriceDirectiveId { get; set; }
    public decimal? OfficialLegalPrice { get; set; }
    public bool IsOfficialPriceExceeded { get; set; }
    public string? ComparisonAnalysisNotes { get; set; }

    // Publication
    public bool IsPubliclyVisible { get; set; }
    public string? PublicSummary { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class ComplaintTrackingResultDto
{
    public string TrackingNumber { get; set; } = string.Empty;
    public string MaskedComplainantName { get; set; } = string.Empty;
    public string ShopOrBusinessName { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Upazila { get; set; } = string.Empty;
    public MarketType SellerType { get; set; }
    public string CropNameBn { get; set; } = string.Empty;
    public decimal QuantityPurchased { get; set; }
    public string ReportedUnit { get; set; } = "কেজি";
    public decimal ActualPriceCharged { get; set; }
    public decimal NormalizedPricePerUnit { get; set; }
    public string StandardUnit { get; set; } = "কেজি";
    public DateTime PurchaseDateTime { get; set; }
    public ComplaintStatus Status { get; set; }
    public string StatusDescriptionBn { get; set; } = string.Empty;
    public DateTime SubmissionDateUtc { get; set; }
    public string? PublicStatusMessage { get; set; }
    public string? ComparisonAnalysisNotes { get; set; }
}

public class ReviewComplaintDto
{
    [Required]
    public Guid ComplaintId { get; set; }

    [Required]
    public ComplaintStatus NewStatus { get; set; }

    [MaxLength(1000)]
    public string? ReviewerNotes { get; set; }

    [MaxLength(500)]
    public string? DecisionReason { get; set; }

    public bool IsPubliclyVisible { get; set; }

    [MaxLength(500)]
    public string? PublicSummary { get; set; }
}

public class ComplaintFilterDto
{
    public string? SearchTerm { get; set; }
    public ComplaintStatus? Status { get; set; }
    public Guid? CropId { get; set; }
    public string? District { get; set; }
    public MarketType? SellerType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
