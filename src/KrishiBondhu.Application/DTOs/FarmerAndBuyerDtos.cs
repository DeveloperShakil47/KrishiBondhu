using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Application.DTOs;

public class FarmerProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullNameBn { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? NationalIdNumber { get; set; }
    public bool IsNidVerified { get; set; }
    public string District { get; set; } = string.Empty;
    public string Upazila { get; set; } = string.Empty;
    public string Union { get; set; } = string.Empty;
    public string VillageOrRoad { get; set; } = string.Empty;
    public decimal TotalLandAreaAcres { get; set; }
    public int FarmingExperienceYears { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? BkashNumber { get; set; }
    public string? NagadNumber { get; set; }
    public List<FarmPlotDto> Plots { get; set; } = new();
}

public class FarmPlotDto
{
    public Guid Id { get; set; }
    public Guid FarmerProfileId { get; set; }

    [Required(ErrorMessage = "জমির খণ্ডের নাম দিন")]
    public string PlotName { get; set; } = string.Empty;

    [Range(0.01, 1000, ErrorMessage = "জমির পরিমাণ সঠিক হতে হবে")]
    public decimal AreaAcres { get; set; }

    public string SoilType { get; set; } = string.Empty;
    public string IrrigationSource { get; set; } = string.Empty;
    public string? CurrentCropName { get; set; }
}

public class BuyerProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string ContactPersonName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public BuyerBusinessType BusinessType { get; set; }
    public string? TradeLicenseNumber { get; set; }
    public bool IsTradeLicenseVerified { get; set; }
    public string District { get; set; } = string.Empty;
    public string Upazila { get; set; } = string.Empty;
    public string WarehouseAddress { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public int CompletedTransactionsCount { get; set; }
}

public class BuyerDemandDto
{
    public Guid Id { get; set; }
    public Guid BuyerProfileId { get; set; }
    public string BuyerBusinessName { get; set; } = string.Empty;
    public Guid CropId { get; set; }
    public string CropNameBn { get; set; } = string.Empty;
    public string DemandTitle { get; set; } = string.Empty;
    public decimal RequiredQuantityKg { get; set; }
    public decimal TargetPricePerKg { get; set; }
    public QualityGrade ExpectedGrade { get; set; }
    public DateTime TargetDeliveryDate { get; set; }
    public string DeliveryDistrict { get; set; } = string.Empty;
    public string DeliveryUpazila { get; set; } = string.Empty;
    public string? QualitySpecifications { get; set; }
    public bool IsFulfilled { get; set; }
}

public class CreateBuyerDemandDto
{
    [Required(ErrorMessage = "ফসলের ধরন নির্বাচন করুন")]
    public Guid CropId { get; set; }

    [Required(ErrorMessage = "চাহিদার শিরোনাম দিন")]
    public string DemandTitle { get; set; } = string.Empty;

    [Range(10, 1000000, ErrorMessage = "চাহিদার পরিমাণ সর্বনিম্ন ১০ কেজি হতে হবে")]
    public decimal RequiredQuantityKg { get; set; }

    [Range(1, 100000, ErrorMessage = "আকাঙ্ক্ষিত মূল্য দিন")]
    public decimal TargetPricePerKg { get; set; }

    public QualityGrade ExpectedGrade { get; set; } = QualityGrade.GradeA;

    [Required(ErrorMessage = "ডেলিভারির তারিখ দিন")]
    public DateTime TargetDeliveryDate { get; set; } = DateTime.UtcNow.AddDays(7);

    [Required(ErrorMessage = "ডেলিভারি জেলা দিন")]
    public string DeliveryDistrict { get; set; } = string.Empty;

    [Required(ErrorMessage = "ডেলিভারি উপজেলা দিন")]
    public string DeliveryUpazila { get; set; } = string.Empty;

    public string? QualitySpecifications { get; set; }
}
