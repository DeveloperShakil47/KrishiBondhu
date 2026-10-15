using KrishiBondhu.Domain.Common;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Domain.Entities;

public class BuyerProfile : BaseEntity
{
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
    public decimal Rating { get; set; } = 5.0m;
    public int CompletedTransactionsCount { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<BuyerDemand> Demands { get; set; } = new List<BuyerDemand>();
    public ICollection<BuyerOffer> Offers { get; set; } = new List<BuyerOffer>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

public class BuyerDemand : BaseEntity
{
    public Guid BuyerProfileId { get; set; }
    public BuyerProfile? BuyerProfile { get; set; }

    public Guid CropId { get; set; }
    public Crop? Crop { get; set; }

    public string DemandTitle { get; set; } = string.Empty;
    public decimal RequiredQuantityKg { get; set; }
    public decimal TargetPricePerKg { get; set; }
    public QualityGrade ExpectedGrade { get; set; }
    public DateTime TargetDeliveryDate { get; set; }
    public string DeliveryDistrict { get; set; } = string.Empty;
    public string DeliveryUpazila { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? QualitySpecifications { get; set; }
    public bool IsFulfilled { get; set; }
    public bool IsActive { get; set; } = true;
}
