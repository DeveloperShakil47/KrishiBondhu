using System;
using System.ComponentModel.DataAnnotations;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Application.DTOs;

public class CropDto
{
    public Guid Id { get; set; }
    public string NameBn { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string DescriptionBn { get; set; } = string.Empty;
    public string StandardUnit { get; set; } = "কেজি";
    public decimal TypicalMinPricePerKg { get; set; }
    public decimal TypicalMaxPricePerKg { get; set; }
    public string? IconEmoji { get; set; }
}

public class HarvestLotDto
{
    public Guid Id { get; set; }
    public string LotNumber { get; set; } = string.Empty;
    public Guid FarmerProfileId { get; set; }
    public string FarmerNameBn { get; set; } = string.Empty;
    public string FarmerPhone { get; set; } = string.Empty;
    public Guid CropId { get; set; }
    public string CropNameBn { get; set; } = string.Empty;
    public string CropCategory { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public WeightUnit Unit { get; set; }
    public decimal QuantityInKg { get; set; }
    public QualityGrade Grade { get; set; }
    public string? GradeDescription { get; set; }
    public decimal AskingPricePerKg { get; set; }
    public decimal? TotalExpectedGrossValue { get; set; }
    public DateTime HarvestDate { get; set; }
    public DateTime AvailableFromDate { get; set; }
    public string PickupDistrict { get; set; } = string.Empty;
    public string PickupUpazila { get; set; } = string.Empty;
    public string PickupLocationDetails { get; set; } = string.Empty;
    public LotStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? PrimaryPhotoUrl { get; set; }
    public int OffersCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class CreateHarvestLotDto
{
    [Required(ErrorMessage = "ফসলের ধরন নির্বাচন করুন")]
    public Guid CropId { get; set; }

    [Range(1, 1000000, ErrorMessage = "পরিমাণ কমপক্ষে ১ হতে হবে")]
    public decimal Quantity { get; set; }

    public WeightUnit Unit { get; set; } = WeightUnit.Mon; // Default to Mon in BD!

    public QualityGrade Grade { get; set; } = QualityGrade.GradeA;
    public string? GradeDescription { get; set; }

    [Range(0.5, 100000, ErrorMessage = "প্রতি কেজির দাম উল্লেখ করুন")]
    public decimal AskingPricePerKg { get; set; }

    public DateTime HarvestDate { get; set; } = DateTime.UtcNow.AddDays(-2);
    public DateTime AvailableFromDate { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "সংগ্রহের জেলা দিন")]
    public string PickupDistrict { get; set; } = string.Empty;

    [Required(ErrorMessage = "সংগ্রহের উপজেলা দিন")]
    public string PickupUpazila { get; set; } = string.Empty;

    public string PickupLocationDetails { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? PrimaryPhotoUrl { get; set; }
}

public class CreateCropDto
{
    [Required(ErrorMessage = "ফসলের বাংলা নাম আবশ্যক")]
    [MaxLength(100, ErrorMessage = "নাম ১০০ অক্ষরের মধ্যে হতে হবে")]
    public string NameBn { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "ইংরেজি নাম ১০০ অক্ষরের মধ্যে হতে হবে")]
    public string NameEn { get; set; } = string.Empty;

    [Required(ErrorMessage = "ফসলের শ্রেণী নির্বাচন করুন")]
    public string Category { get; set; } = "শাকসবজি (Vegetables)";

    public string DescriptionBn { get; set; } = string.Empty;

    public string StandardUnit { get; set; } = "কেজি";

    [Range(0, 100000, ErrorMessage = "প্রত্যাশিত সর্বনিম্ন দর সঠিক দিন")]
    public decimal TypicalMinPricePerKg { get; set; } = 20m;

    [Range(0, 100000, ErrorMessage = "প্রত্যাশিত সর্বোচ্চ দর সঠিক দিন")]
    public decimal TypicalMaxPricePerKg { get; set; } = 50m;

    public string? IconEmoji { get; set; } = "🌱";
}
