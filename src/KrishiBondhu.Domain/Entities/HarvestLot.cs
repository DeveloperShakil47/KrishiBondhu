using KrishiBondhu.Domain.Common;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Domain.Entities;

public class HarvestLot : BaseEntity
{
    public string LotNumber { get; set; } = string.Empty; // e.g., "LOT-2026-BOG-001"

    public Guid FarmerProfileId { get; set; }
    public FarmerProfile? FarmerProfile { get; set; }

    public Guid CropId { get; set; }
    public Crop? Crop { get; set; }

    public decimal Quantity { get; set; }
    public WeightUnit Unit { get; set; } = WeightUnit.Kg;

    /// <summary>
    /// Normalized quantity in Kilograms (1 Mon = 40 Kg)
    /// </summary>
    public decimal QuantityInKg => Unit == WeightUnit.Mon ? Quantity * 40m : Quantity;

    public QualityGrade Grade { get; set; } = QualityGrade.Standard;
    public string? GradeDescription { get; set; }

    public decimal AskingPricePerKg { get; set; }
    public decimal? TotalExpectedGrossValue => QuantityInKg * AskingPricePerKg;

    public DateTime HarvestDate { get; set; }
    public DateTime AvailableFromDate { get; set; }

    public string PickupDistrict { get; set; } = string.Empty;
    public string PickupUpazila { get; set; } = string.Empty;
    public string PickupLocationDetails { get; set; } = string.Empty;

    public LotStatus Status { get; set; } = LotStatus.ActiveListed;
    public string? Notes { get; set; }
    public string? PrimaryPhotoUrl { get; set; }

    // Navigation
    public ICollection<BuyerOffer> Offers { get; set; } = new List<BuyerOffer>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
