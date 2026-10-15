using KrishiBondhu.Domain.Common;

namespace KrishiBondhu.Domain.Entities;

public class FarmerProfile : BaseEntity
{
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
    public string PreferredLanguage { get; set; } = "bn";
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? BkashNumber { get; set; }
    public string? NagadNumber { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<FarmPlot> Plots { get; set; } = new List<FarmPlot>();
    public ICollection<HarvestLot> HarvestLots { get; set; } = new List<HarvestLot>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

public class FarmPlot : BaseEntity
{
    public Guid FarmerProfileId { get; set; }
    public FarmerProfile? FarmerProfile { get; set; }

    public string PlotName { get; set; } = string.Empty;
    public decimal AreaAcres { get; set; }
    public string SoilType { get; set; } = string.Empty; // বেলে দোআঁশ, এঁটেল, পলি
    public string IrrigationSource { get; set; } = string.Empty; // গভীর নলকূপ, খাল, বৃষ্টির পানি
    public string? CurrentCropName { get; set; }
    public string? MouzaAndKhatianNumber { get; set; }
}
