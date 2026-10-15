using System;
using System.ComponentModel.DataAnnotations;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Application.DTOs;

public class PaymentRecordDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public DateTime PaymentDateUtc { get; set; }
    public string? SenderAccountOrPhone { get; set; }
    public string? ReceiverAccountOrPhone { get; set; }
    public string? ReceiptEvidenceUrl { get; set; }
    public PaymentStatus Status { get; set; }
    public string? VerifiedByRole { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }
    public string? VerificationNotes { get; set; }
}

public class RecordPaymentDto
{
    [Required]
    public Guid OrderId { get; set; }

    [Range(1, 10000000, ErrorMessage = "পরিশোধিত টাকার পরিমাণ সঠিক হতে হবে")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "পেমেন্ট মাধ্যম নির্বাচন করুন")]
    public PaymentMethod Method { get; set; } = PaymentMethod.bKash;

    [Required(ErrorMessage = "ট্রানজ্যাকশন আইডি বা ভাউচার নম্বর দিন")]
    public string TransactionReference { get; set; } = string.Empty;

    public string? SenderAccountOrPhone { get; set; }
    public string? ReceiverAccountOrPhone { get; set; }
    public string? ReceiptEvidenceUrl { get; set; }
}

public class VerifyPaymentDto
{
    [Required]
    public Guid PaymentRecordId { get; set; }

    public bool IsApproved { get; set; }

    [Required(ErrorMessage = "যাচাইকরণ নোট বা কারণ উল্লেখ করুন")]
    public string VerificationNotes { get; set; } = string.Empty;
}

public class DisputeCaseDto
{
    public Guid Id { get; set; }
    public string DisputeNumber { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string RaisedByName { get; set; } = string.Empty;
    public string RaisedByRole { get; set; } = string.Empty;
    public DisputeReason Reason { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal? DisputedAmount { get; set; }
    public decimal? DisputedQuantityKg { get; set; }
    public DisputeStatus Status { get; set; }
    public string? AssignedToName { get; set; }
    public string? ResolutionNotes { get; set; }
    public decimal? FinalAdjustedAmount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
}

public class RaiseDisputeDto
{
    [Required]
    public Guid OrderId { get; set; }

    [Required(ErrorMessage = "বিরোধের কারণ নির্বাচন করুন")]
    public DisputeReason Reason { get; set; }

    [Required(ErrorMessage = "বিস্তারিত বিবরণ প্রদান করুন")]
    [MinLength(10, ErrorMessage = "বিবরণ অন্তত ১০ অক্ষরের হতে হবে")]
    public string Description { get; set; } = string.Empty;

    public decimal? DisputedAmount { get; set; }
    public decimal? DisputedQuantityKg { get; set; }
}

public class ResolveDisputeDto
{
    [Required]
    public Guid DisputeId { get; set; }

    public bool IsResolved { get; set; } // true = Resolved, false = Rejected

    [Required(ErrorMessage = "মীমাংসা সংক্রান্ত নোট দিন")]
    public string ResolutionNotes { get; set; } = string.Empty;

    public decimal? FinalAdjustedAmount { get; set; }
}
