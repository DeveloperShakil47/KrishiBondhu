using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Application.DTOs;

public class MarketDto
{
    public Guid Id { get; set; }
    public string MarketCode { get; set; } = string.Empty;
    public string NameBn { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public MarketType MarketType { get; set; }
    public string District { get; set; } = string.Empty;
    public string Upazila { get; set; } = string.Empty;
    public string? AddressDetails { get; set; }
    public bool IsActive { get; set; }
}

public class PriceSourceDto
{
    public Guid Id { get; set; }
    public string NameBn { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public PriceSourceType SourceType { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public bool IsOfficialAuthority { get; set; }
    public string? Description { get; set; }
}

public class OfficialPriceDirectiveDto
{
    public Guid Id { get; set; }
    public string DirectiveNumber { get; set; } = string.Empty;
    public string TitleBn { get; set; } = string.Empty;
    public string IssuingAuthority { get; set; } = string.Empty;
    public string SourceReference { get; set; } = string.Empty;
    public Guid CropId { get; set; }
    public string CropNameBn { get; set; } = string.Empty;
    public MarketType MarketType { get; set; }
    public string? ApplicableDistrict { get; set; }
    public string? ApplicableUpazila { get; set; }
    public DateTime EffectiveFromUtc { get; set; }
    public DateTime? EffectiveToUtc { get; set; }
    public decimal OfficialBenchmarkPrice { get; set; }
    public decimal LegalMaxPrice { get; set; }
    public string StandardUnit { get; set; } = "কেজি";
    public string? ApplicableConditions { get; set; }
    public PriceVerificationStatus VerificationStatus { get; set; }
    public bool IsActive { get; set; }
}

public class MarketPriceObservationDto
{
    public Guid Id { get; set; }
    public Guid CropId { get; set; }
    public string CropNameBn { get; set; } = string.Empty;
    public string CropCategory { get; set; } = string.Empty;
    public Guid MarketId { get; set; }
    public string MarketNameBn { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Upazila { get; set; } = string.Empty;
    public MarketType MarketType { get; set; }
    public DateTime ObservationDate { get; set; }
    public Guid PriceSourceId { get; set; }
    public string SourceNameBn { get; set; } = string.Empty;
    public decimal ObservedPrice { get; set; }
    public decimal ObservedQuantity { get; set; }
    public string ObservedUnit { get; set; } = "কেজি";
    public decimal NormalizedPricePerUnit { get; set; }
    public string NormalizedUnit { get; set; } = "কেজি";
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public QualityGrade Grade { get; set; }
    public PriceVerificationStatus VerificationStatus { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class CreateMarketPriceObservationDto
{
    [Required(ErrorMessage = "ফসল নির্বাচন করুন")]
    public Guid CropId { get; set; }

    [Required(ErrorMessage = "বাজার নির্বাচন করুন")]
    public Guid MarketId { get; set; }

    [Required(ErrorMessage = "বাজারের ধরন নির্বাচন করুন")]
    public MarketType MarketType { get; set; } = MarketType.Wholesale;

    [Required(ErrorMessage = "তারিখ নির্বাচন করুন")]
    public DateTime ObservationDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "তথ্যের উৎস নির্বাচন করুন")]
    public Guid PriceSourceId { get; set; }

    [Range(0.01, 10000000, ErrorMessage = "মূল্য ০ এর বেশি হতে হবে")]
    public decimal ObservedPrice { get; set; }

    [Range(0.01, 100000, ErrorMessage = "পরিমাণ ০ এর বেশি হতে হবে")]
    public decimal ObservedQuantity { get; set; } = 1.0m;

    [Required(ErrorMessage = "পরিমাপের একক লিখুন বা নির্বাচন করুন")]
    public string ObservedUnit { get; set; } = "কেজি";

    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    public QualityGrade Grade { get; set; } = QualityGrade.Standard;
    public string? Notes { get; set; }
}

public class MarketPriceFilterDto
{
    public Guid? CropId { get; set; }
    public string? Category { get; set; }
    public MarketType? MarketType { get; set; }
    public string? District { get; set; }
    public string? Upazila { get; set; }
    public Guid? MarketId { get; set; }
    public DateTime? Date { get; set; }
    public Guid? PriceSourceId { get; set; }
    public PriceVerificationStatus? VerificationStatus { get; set; } = PriceVerificationStatus.Verified;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class DailyPriceMetricsDto
{
    public decimal HighestPrice { get; set; }
    public decimal LowestPrice { get; set; }
    public decimal AveragePrice { get; set; }
    public int ObservationCount { get; set; }
}

public class PriceComparisonDeltaDto
{
    public decimal CurrentAverage { get; set; }
    public decimal PreviousDayAverage { get; set; }
    public decimal PriceDifference { get; set; }
    public decimal PercentageChange { get; set; }
    public bool HasHistoricalData { get; set; }
}

public class MarketPriceSummaryDto
{
    public Guid CropId { get; set; }
    public string CropNameBn { get; set; } = string.Empty;
    public string CropCategory { get; set; } = string.Empty;
    public string StandardUnit { get; set; } = "কেজি";
    public DateTime SummaryDate { get; set; }

    // Wholesale Metrics
    public DailyPriceMetricsDto Wholesale { get; set; } = new();

    // Retail Metrics
    public DailyPriceMetricsDto Retail { get; set; } = new();

    // Previous Day Comparison
    public PriceComparisonDeltaDto WholesaleComparison { get; set; } = new();
    public PriceComparisonDeltaDto RetailComparison { get; set; } = new();

    // Officially Published Government Directives (Statutory limits or benchmark prices)
    public List<OfficialPriceDirectiveDto> ApplicableOfficialDirectives { get; set; } = new();
}

public class PriceTrendPointDto
{
    public DateTime Date { get; set; }
    public string FormattedDate { get; set; } = string.Empty;
    public decimal? AverageWholesalePrice { get; set; }
    public decimal? AverageRetailPrice { get; set; }
    public decimal? LowestObservedPrice { get; set; }
    public decimal? HighestObservedPrice { get; set; }
    public int TotalObservations { get; set; }
}
