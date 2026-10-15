using KrishiBondhu.Domain.Common;
using KrishiBondhu.Domain.Enums;
using KrishiBondhu.Domain.Exceptions;

namespace KrishiBondhu.Domain.Entities;

public class WeighingRecord : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public int SequenceNumber { get; set; } = 1; // 1 for first weigh, 2+ for re-weigh / correction

    public decimal GrossWeight { get; set; }
    public decimal TareWeight { get; set; }
    public decimal NetWeight { get; set; }

    public WeightUnit Unit { get; set; } = WeightUnit.Kg;

    /// <summary>
    /// Calculated Net Weight in Kilograms (1 Mon = 40 Kg)
    /// </summary>
    public decimal NetWeightInKg => Unit == WeightUnit.Mon ? NetWeight * 40m : NetWeight;

    public string CollectionPointName { get; set; } = string.Empty;
    public string CollectionDistrict { get; set; } = string.Empty;
    public string CollectionUpazila { get; set; } = string.Empty;

    public Guid? OperatorUserId { get; set; }
    public string? OperatorFullName { get; set; }

    public string? ScaleIdentifier { get; set; } // e.g. "DIGI-SC-202"
    public string? ScaleCalibrationStatus { get; set; } // "BSTI Verified / Valid to 2027"

    public WeighingStatus Status { get; set; } = WeighingStatus.Recorded;

    public bool IsFarmerConfirmed { get; set; }
    public DateTime? FarmerConfirmedAtUtc { get; set; }

    public bool IsBuyerConfirmed { get; set; }
    public DateTime? BuyerConfirmedAtUtc { get; set; }

    public bool IsCorrectedRecord { get; set; }
    public Guid? SupersededRecordId { get; set; }
    public string? CorrectionReason { get; set; }

    public string? ReceiptPhotoUrl { get; set; }
    public string? Remarks { get; set; }

    public DateTime RecordedAtUtc { get; set; } = DateTime.UtcNow;

    public static WeighingRecord Create(
        Guid orderId,
        decimal grossWeight,
        decimal tareWeight,
        WeightUnit unit,
        string collectionPointName,
        string district,
        string upazila,
        Guid? operatorUserId = null,
        string? operatorFullName = null,
        string? scaleId = null,
        string? calibrationStatus = null,
        string? remarks = null)
    {
        if (grossWeight <= 0)
        {
            throw new InvalidWeightException("মোট ওজন (Gross Weight) অবশ্যই শূন্যের চেয়ে বেশি হতে হবে।");
        }

        if (tareWeight < 0)
        {
            throw new InvalidWeightException("পাত্র বা বস্তার ওজন (Tare Weight) ঋণাত্মক হতে পারে না।");
        }

        if (grossWeight <= tareWeight)
        {
            throw new InvalidWeightException("মোট ওজন অবশ্যই পাত্র/বস্তার ওজনের চেয়ে বেশি হতে হবে (Gross must exceed Tare).");
        }

        decimal net = Math.Round(grossWeight - tareWeight, 2, MidpointRounding.AwayFromZero);

        return new WeighingRecord
        {
            OrderId = orderId,
            GrossWeight = grossWeight,
            TareWeight = tareWeight,
            NetWeight = net,
            Unit = unit,
            CollectionPointName = collectionPointName,
            CollectionDistrict = district,
            CollectionUpazila = upazila,
            OperatorUserId = operatorUserId,
            OperatorFullName = operatorFullName,
            ScaleIdentifier = scaleId,
            ScaleCalibrationStatus = calibrationStatus,
            Remarks = remarks,
            RecordedAtUtc = DateTime.UtcNow,
            Status = WeighingStatus.Recorded
        };
    }
}
