using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Application.DTOs;

public class BuyerOfferDto
{
    public Guid Id { get; set; }
    public string OfferCode { get; set; } = string.Empty;
    public Guid HarvestLotId { get; set; }
    public string LotNumber { get; set; } = string.Empty;
    public string CropNameBn { get; set; } = string.Empty;
    public Guid BuyerProfileId { get; set; }
    public string BuyerBusinessName { get; set; } = string.Empty;
    public string BuyerContactPhone { get; set; } = string.Empty;
    public decimal BuyerRating { get; set; }

    public decimal OfferedPricePerKg { get; set; }
    public decimal OfferedQuantityKg { get; set; }

    // Itemized Deductions
    public decimal TransportCostDeduction { get; set; }
    public decimal WeighingAndLaborCostDeduction { get; set; }
    public decimal PackagingCostDeduction { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal OtherDeductions { get; set; }

    public decimal GrossSaleValue { get; set; }
    public decimal TotalEstimatedDeductions { get; set; }
    public decimal EstimatedNetProceeds { get; set; }

    public DateTime ExpiryDateUtc { get; set; }
    public string DeliveryTerms { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public OfferStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class CreateBuyerOfferDto
{
    [Required]
    public Guid HarvestLotId { get; set; }

    [Range(0.5, 100000, ErrorMessage = "প্রস্তাবিত প্রতি কেজির দর দিন")]
    public decimal OfferedPricePerKg { get; set; }

    [Range(1, 1000000, ErrorMessage = "প্রস্তাবিত মোট পরিমাণ (কেজি) দিন")]
    public decimal OfferedQuantityKg { get; set; }

    [Range(0, 500000, ErrorMessage = "পরিবহন খরচ কর্তন সঠিক হতে হবে")]
    public decimal TransportCostDeduction { get; set; }

    [Range(0, 500000, ErrorMessage = "ওজন ও কুলি খরচ সঠিক হতে হবে")]
    public decimal WeighingAndLaborCostDeduction { get; set; }

    [Range(0, 500000, ErrorMessage = "প্যাকেজিং খরচ সঠিক হতে হবে")]
    public decimal PackagingCostDeduction { get; set; }

    public decimal OtherDeductions { get; set; }

    [Required(ErrorMessage = "প্রস্তাবের মেয়াদের সময়সীমা দিন")]
    public DateTime ExpiryDateUtc { get; set; } = DateTime.UtcNow.AddDays(3);

    [Required(ErrorMessage = "ডেলিভারি শর্তাবলী উল্লেখ করুন")]
    public string DeliveryTerms { get; set; } = "খামার গেট থেকে পরিবহন (Farmgate Pickup)";

    [Required(ErrorMessage = "পেমেন্ট শর্তাবলী উল্লেখ করুন")]
    public string PaymentTerms { get; set; } = "ওজন নিশ্চিতকরণের সাথে সাথে বিকাশ/ব্যাংক ট্র্যান্সফার";

    public string? Remarks { get; set; }
}

public class OfferComparisonSummaryDto
{
    public Guid HarvestLotId { get; set; }
    public string LotNumber { get; set; } = string.Empty;
    public string CropNameBn { get; set; } = string.Empty;
    public decimal LotQuantityKg { get; set; }
    public decimal AskingPricePerKg { get; set; }
    public decimal FarmerEstimatedProductionCostPerKg { get; set; } // Optional user input for true profit calculation

    public List<BuyerOfferDto> Offers { get; set; } = new();

    /// <summary>
    /// Highest net proceeds offer
    /// </summary>
    public BuyerOfferDto? RecommendedBestOffer { get; set; }
}
