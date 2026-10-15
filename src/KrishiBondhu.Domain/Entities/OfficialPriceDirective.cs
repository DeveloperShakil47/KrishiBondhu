using System;
using KrishiBondhu.Domain.Common;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Domain.Entities;

public class OfficialPriceDirective : BaseEntity
{
    public string DirectiveNumber { get; set; } = string.Empty;
    public string TitleBn { get; set; } = string.Empty;
    public string IssuingAuthority { get; set; } = string.Empty; // e.g. জাতীয় ভোক্তা-অধিকার সংরক্ষণ অধিদপ্তর, কৃষি বিপণন অধিদপ্তর
    public string SourceReference { get; set; } = string.Empty; // e.g. স্মারক নং: ৩৮.০১.০০০০.০২৭.১২.০৩৪.২৬
    
    public Guid CropId { get; set; }
    public Crop? Crop { get; set; }

    public MarketType MarketType { get; set; } = MarketType.Both; // Wholesale, Retail, Both
    
    public string? ApplicableDistrict { get; set; } // Null if nationwide
    public string? ApplicableUpazila { get; set; } // Null if all upazilas in district or nationwide

    public DateTime EffectiveFromUtc { get; set; }
    public DateTime? EffectiveToUtc { get; set; }

    // Official Benchmark or Statutory maximum price limit
    public decimal OfficialBenchmarkPrice { get; set; }
    public decimal LegalMaxPrice { get; set; }
    public string StandardUnit { get; set; } = "কেজি";

    public string? ApplicableConditions { get; set; } // e.g. "খোলা সয়াবিন তেল মিলগেট ও পাইকারি পর্যায়ে প্রযোজ্য"
    
    public PriceVerificationStatus VerificationStatus { get; set; } = PriceVerificationStatus.Verified;
    public string? VerifiedByUserId { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Evaluates whether this official price directive is legally applicable to a specific transaction or observation.
    /// Never treats market highs as legal maximums; only this explicit directive defines statutory bounds.
    /// </summary>
    public bool IsApplicable(Guid cropId, MarketType marketType, string district, string? upazila, DateTime date, string unit)
    {
        if (!IsActive) return false;
        if (VerificationStatus != PriceVerificationStatus.Verified) return false;
        if (CropId != cropId) return false;

        // Market type match
        if (MarketType != MarketType.Both && MarketType != marketType)
            return false;

        // Date range match
        var dateUtc = date.Kind == DateTimeKind.Utc ? date : date.ToUniversalTime();
        if (dateUtc < EffectiveFromUtc) return false;
        if (EffectiveToUtc.HasValue && dateUtc > EffectiveToUtc.Value) return false;

        // Geographical match (null means nationwide)
        if (!string.IsNullOrWhiteSpace(ApplicableDistrict))
        {
            if (!string.Equals(ApplicableDistrict.Trim(), district.Trim(), StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.IsNullOrWhiteSpace(ApplicableUpazila) && !string.IsNullOrWhiteSpace(upazila))
            {
                if (!string.Equals(ApplicableUpazila.Trim(), upazila.Trim(), StringComparison.OrdinalIgnoreCase))
                    return false;
            }
        }

        // Unit compatibility
        if (!UnitNormalizer.AreCompatible(StandardUnit, unit))
            return false;

        return true;
    }
}
