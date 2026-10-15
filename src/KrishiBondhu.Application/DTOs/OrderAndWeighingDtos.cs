using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Application.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid HarvestLotId { get; set; }
    public string LotNumber { get; set; } = string.Empty;
    public string CropNameBn { get; set; } = string.Empty;

    public Guid FarmerProfileId { get; set; }
    public string FarmerNameBn { get; set; } = string.Empty;
    public string FarmerPhoneNumber { get; set; } = string.Empty;

    public Guid BuyerProfileId { get; set; }
    public string BuyerBusinessName { get; set; } = string.Empty;
    public string BuyerPhoneNumber { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }

    // Agreed Terms Snapshot
    public decimal AgreedPricePerKg { get; set; }
    public decimal AgreedQuantityKg { get; set; }
    public QualityGrade AgreedGrade { get; set; }
    public string AgreedDeliveryTerms { get; set; } = string.Empty;
    public string AgreedPaymentTerms { get; set; } = string.Empty;

    public decimal AgreedTransportDeduction { get; set; }
    public decimal AgreedWeighingDeduction { get; set; }
    public decimal AgreedPackagingDeduction { get; set; }
    public decimal AgreedPlatformFee { get; set; }
    public decimal AgreedOtherDeductions { get; set; }

    // Calculated amounts
    public decimal? VerifiedNetQuantityKg { get; set; }
    public decimal EffectiveQuantityKg { get; set; }
    public decimal TotalGrossAmount { get; set; }
    public decimal TotalAgreedDeductions { get; set; }
    public decimal NetPayableToFarmer { get; set; }
    public decimal VerifiedPaidAmount { get; set; }
    public decimal OutstandingBalance { get; set; }

    public DateTime AcceptedAtUtc { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public string? CancellationReason { get; set; }

    public List<WeighingRecordDto> WeighingRecords { get; set; } = new();
    public List<PaymentRecordDto> PaymentRecords { get; set; } = new();
    public List<DisputeCaseDto> Disputes { get; set; } = new();
}

public class AcceptOfferDto
{
    [Required]
    public Guid BuyerOfferId { get; set; }

    [Required]
    public string IdempotencyKey { get; set; } = Guid.NewGuid().ToString();
}

public class WeighingRecordDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public int SequenceNumber { get; set; }
    public decimal GrossWeight { get; set; }
    public decimal TareWeight { get; set; }
    public decimal NetWeight { get; set; }
    public WeightUnit Unit { get; set; }
    public decimal NetWeightInKg { get; set; }
    public string CollectionPointName { get; set; } = string.Empty;
    public string CollectionDistrict { get; set; } = string.Empty;
    public string CollectionUpazila { get; set; } = string.Empty;
    public string? OperatorFullName { get; set; }
    public string? ScaleIdentifier { get; set; }
    public string? ScaleCalibrationStatus { get; set; }
    public WeighingStatus Status { get; set; }
    public bool IsFarmerConfirmed { get; set; }
    public bool IsBuyerConfirmed { get; set; }
    public bool IsCorrectedRecord { get; set; }
    public string? CorrectionReason { get; set; }
    public string? ReceiptPhotoUrl { get; set; }
    public string? Remarks { get; set; }
    public DateTime RecordedAtUtc { get; set; }
}

public class RecordWeighingDto
{
    [Required]
    public Guid OrderId { get; set; }

    [Range(0.1, 1000000, ErrorMessage = "মোট ওজন (Gross Weight) শূন্যের বেশি হতে হবে")]
    public decimal GrossWeight { get; set; }

    [Range(0, 500000, ErrorMessage = "পাত্র/বস্তার ওজন (Tare Weight) ঋণাত্মক হতে পারে না")]
    public decimal TareWeight { get; set; }

    public WeightUnit Unit { get; set; } = WeightUnit.Kg;

    [Required(ErrorMessage = "ওজন মাপার স্থান উল্লেখ করুন")]
    public string CollectionPointName { get; set; } = string.Empty;

    [Required(ErrorMessage = "জেলা উল্লেখ করুন")]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "উপজেলা উল্লেখ করুন")]
    public string Upazila { get; set; } = string.Empty;

    public string? ScaleIdentifier { get; set; }
    public string? ScaleCalibrationStatus { get; set; }
    public string? Remarks { get; set; }
    public string? ReceiptPhotoUrl { get; set; }
}
