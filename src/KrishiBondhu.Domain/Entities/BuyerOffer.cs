using KrishiBondhu.Domain.Common;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Domain.Entities;

public class BuyerOffer : BaseEntity
{
    public string OfferCode { get; set; } = string.Empty; // e.g., "OFR-2026-0001"

    public Guid HarvestLotId { get; set; }
    public HarvestLot? HarvestLot { get; set; }

    public Guid BuyerProfileId { get; set; }
    public BuyerProfile? BuyerProfile { get; set; }

    public decimal OfferedPricePerKg { get; set; }
    public decimal OfferedQuantityKg { get; set; }

    // Cost deductions borne by or applicable to the seller
    public decimal TransportCostDeduction { get; set; }
    public decimal WeighingAndLaborCostDeduction { get; set; }
    public decimal PackagingCostDeduction { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal OtherDeductions { get; set; }

    public DateTime ExpiryDateUtc { get; set; }
    public string DeliveryTerms { get; set; } = string.Empty; // e.g., "কৃষকের খামার গেট থেকে সংগ্রহ (Farmgate Pickup)"
    public string PaymentTerms { get; set; } = string.Empty; // e.g., "ওজন নিশ্চিতকরণের পর ৩ ঘণ্টার মধ্যে বিকাশ/ব্যাংক"
    public string? Remarks { get; set; }

    public OfferStatus Status { get; set; } = OfferStatus.Submitted;
    public DateTime? RespondedAtUtc { get; set; }
    public string? FarmerResponseNote { get; set; }

    // Calculated fields
    public decimal GrossSaleValue => Math.Round(OfferedQuantityKg * OfferedPricePerKg, 2, MidpointRounding.AwayFromZero);

    public decimal TotalEstimatedDeductions =>
        Math.Round(TransportCostDeduction + WeighingAndLaborCostDeduction + PackagingCostDeduction + PlatformFee + OtherDeductions, 2, MidpointRounding.AwayFromZero);

    public decimal EstimatedNetProceeds =>
        Math.Round(GrossSaleValue - TotalEstimatedDeductions, 2, MidpointRounding.AwayFromZero);

    // Navigation
    public ICollection<OfferCost> ItemizedCosts { get; set; } = new List<OfferCost>();
    public Order? ResultingOrder { get; set; }
}

public class OfferCost : BaseEntity
{
    public Guid BuyerOfferId { get; set; }
    public BuyerOffer? BuyerOffer { get; set; }

    public CostType CostType { get; set; }
    public string DescriptionBn { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool BorneByFarmer { get; set; } = true;
}
