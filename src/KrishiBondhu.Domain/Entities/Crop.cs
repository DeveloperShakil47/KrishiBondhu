using KrishiBondhu.Domain.Common;

namespace KrishiBondhu.Domain.Entities;

public class Crop : BaseEntity
{
    public string NameBn { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // e.g., খাদ্যশস্য (Cereals), ডাল (Pulses), শাকসবজি (Vegetables), মসলা (Spices), তৈলবীজ (Oilseeds)
    public string DescriptionBn { get; set; } = string.Empty;
    public string StandardUnit { get; set; } = "কেজি";
    public decimal TypicalMinPricePerKg { get; set; }
    public decimal TypicalMaxPricePerKg { get; set; }
    public string? IconEmoji { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<HarvestLot> HarvestLots { get; set; } = new List<HarvestLot>();
    public ICollection<BuyerDemand> BuyerDemands { get; set; } = new List<BuyerDemand>();
}
