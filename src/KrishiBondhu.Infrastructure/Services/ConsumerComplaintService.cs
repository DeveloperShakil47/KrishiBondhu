using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KrishiBondhu.Application.Common.Models;
using KrishiBondhu.Application.DTOs;
using KrishiBondhu.Application.Interfaces.Repositories;
using KrishiBondhu.Application.Interfaces.Services;
using KrishiBondhu.Domain.Common;
using KrishiBondhu.Domain.Entities;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Infrastructure.Services;

public class ConsumerComplaintService : IConsumerComplaintService
{
    private readonly IConsumerComplaintRepository _complaintRepo;
    private readonly IMarketPriceObservationRepository _observationRepo;
    private readonly IOfficialPriceDirectiveRepository _directiveRepo;
    private readonly ICropRepository _cropRepo;
    private readonly IMarketRepository _marketRepo;
    private readonly IAuditLogRepository _auditLogRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ConsumerComplaintService(
        IConsumerComplaintRepository complaintRepo,
        IMarketPriceObservationRepository observationRepo,
        IOfficialPriceDirectiveRepository directiveRepo,
        ICropRepository cropRepo,
        IMarketRepository marketRepo,
        IAuditLogRepository auditLogRepo,
        IUnitOfWork unitOfWork)
    {
        _complaintRepo = complaintRepo;
        _observationRepo = observationRepo;
        _directiveRepo = directiveRepo;
        _cropRepo = cropRepo;
        _marketRepo = marketRepo;
        _auditLogRepo = auditLogRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ConsumerComplaintDto>> SubmitComplaintAsync(
        SubmitConsumerComplaintDto dto,
        string? complainantUserId = null,
        string? clientIp = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Validation
        if (dto.ActualPriceCharged <= 0)
            return Result<ConsumerComplaintDto>.Failure("প্রকৃত মূল্য ০ এর বেশি হতে হবে।");

        if (dto.QuantityPurchased <= 0)
            return Result<ConsumerComplaintDto>.Failure("পরিমাণ ০ এর বেশি হতে হবে।");

        var crop = await _cropRepo.GetByIdAsync(dto.CropId, cancellationToken);
        if (crop == null)
            return Result<ConsumerComplaintDto>.Failure("নির্বাচিত পণ্য খুঁজে পাওয়া যায়নি।");

        if (!UnitNormalizer.AreCompatible(dto.ReportedUnit, crop.StandardUnit))
        {
            return Result<ConsumerComplaintDto>.Failure($"পরিমাপের একক পণ্যের ধরন ({crop.StandardUnit})-এর সাথে অসঙ্গতিপূর্ণ।");
        }

        // 2. Client IP Hash & Rate Limiting (Spam Detection)
        string? ipHash = null;
        if (!string.IsNullOrWhiteSpace(clientIp))
        {
            ipHash = HashString(clientIp);
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);
            var recentCount = await _complaintRepo.GetRecentSubmissionsCountByIpAsync(ipHash, oneHourAgo, cancellationToken);
            if (recentCount >= 5)
            {
                return Result<ConsumerComplaintDto>.Failure("একই নেটওয়ার্ক থেকে সংক্ষিপ্ত সময়ে একাধিক অভিযোগ দাখিল করা হয়েছে। অনুগ্রহ করে কিছুক্ষণ পর চেষ্টা করুন।");
            }
        }

        // 3. Server-side normalized price calculation (never trust client-computed amounts)
        var (normalizedPrice, baseUnit) = UnitNormalizer.CalculateNormalizedPrice(
            dto.ActualPriceCharged, dto.QuantityPurchased, dto.ReportedUnit);

        // 4. Generate unique tracking number
        var trackingNumber = await _complaintRepo.GenerateNextTrackingNumberAsync(cancellationToken);

        // 5. Market price matching (objective, factual comparison)
        var observations = await _observationRepo.GetObservationsForCropAndDateAsync(
            dto.CropId, dto.PurchaseDateTime.Date, dto.SellerType, dto.District, cancellationToken);

        decimal? avgMarketPrice = null;
        decimal? minMarketPrice = null;
        decimal? maxMarketPrice = null;
        decimal? variancePct = null;

        if (observations.Any())
        {
            avgMarketPrice = Math.Round(observations.Average(o => o.NormalizedPricePerUnit), 2);
            minMarketPrice = observations.Min(o => o.NormalizedPricePerUnit);
            maxMarketPrice = observations.Max(o => o.NormalizedPricePerUnit);

            if (avgMarketPrice.Value > 0)
            {
                variancePct = Math.Round(((normalizedPrice - avgMarketPrice.Value) / avgMarketPrice.Value) * 100m, 2);
            }
        }

        // 6. Official Legal Directives check
        var directives = await _directiveRepo.GetActiveDirectivesForCropAsync(dto.CropId, dto.PurchaseDateTime, cancellationToken);
        var applicableDirective = directives.FirstOrDefault(d =>
            d.IsApplicable(dto.CropId, dto.SellerType, dto.District, dto.Upazila, dto.PurchaseDateTime, dto.ReportedUnit));

        Guid? directiveId = applicableDirective?.Id;
        decimal? legalPrice = applicableDirective?.LegalMaxPrice;
        bool isOfficialPriceExceeded = false;

        if (applicableDirective != null && applicableDirective.LegalMaxPrice > 0)
        {
            isOfficialPriceExceeded = normalizedPrice > applicableDirective.LegalMaxPrice;
        }

        // 7. Generate neutral, factual comparison analysis
        var analysisNotes = GenerateNeutralAnalysis(normalizedPrice, baseUnit, avgMarketPrice, minMarketPrice, maxMarketPrice, variancePct, applicableDirective, isOfficialPriceExceeded);

        // 8. Create complaint entity (Strictly private by default)
        var complaint = new ConsumerComplaint
        {
            TrackingNumber = trackingNumber,
            ComplainantUserId = complainantUserId,
            ComplainantName = dto.ComplainantName.Trim(),
            ComplainantPhone = dto.ComplainantPhone.Trim(),
            ComplainantEmail = dto.ComplainantEmail?.Trim(),
            ShopOrBusinessName = dto.ShopOrBusinessName.Trim(),
            ShopAddress = dto.ShopAddress.Trim(),
            MarketId = dto.MarketId,
            District = dto.District.Trim(),
            Upazila = dto.Upazila.Trim(),
            SellerType = dto.SellerType,
            CropId = dto.CropId,
            QuantityPurchased = dto.QuantityPurchased,
            ReportedUnit = UnitNormalizer.CanonicalizeUnit(dto.ReportedUnit),
            ActualPriceCharged = dto.ActualPriceCharged,
            NormalizedPricePerUnit = normalizedPrice,
            StandardUnit = baseUnit,
            PurchaseDateTime = dto.PurchaseDateTime,
            Description = dto.Description,
            ReceiptEvidencePath = dto.ReceiptEvidencePath,
            ReceiptFileName = dto.ReceiptFileName,
            ReceiptFileSizeBytes = dto.ReceiptFileSizeBytes,
            ReceiptContentType = dto.ReceiptContentType,
            Status = ComplaintStatus.Submitted,
            MatchedMarketAveragePrice = avgMarketPrice,
            MatchedMarketMinPrice = minMarketPrice,
            MatchedMarketMaxPrice = maxMarketPrice,
            PriceVariancePercentage = variancePct,
            OfficialPriceDirectiveId = directiveId,
            OfficialLegalPrice = legalPrice,
            IsOfficialPriceExceeded = isOfficialPriceExceeded,
            ComparisonAnalysisNotes = analysisNotes,
            IsPubliclyVisible = false,
            ClientIpHash = ipHash,
            UserAgent = userAgent,
            CreatedBy = complainantUserId ?? "PublicUser"
        };

        await _complaintRepo.AddAsync(complaint, cancellationToken);

        // Audit log
        var audit = new AuditLog
        {
            EntityName = nameof(ConsumerComplaint),
            EntityId = complaint.Id.ToString(),
            Action = "ComplaintSubmitted",
            PerformedByUserId = Guid.TryParse(complainantUserId, out var uid) ? uid : null,
            DetailsJson = $"ভোক্তা অভিযোগ দাখিল: ট্র্যাকিং নং {trackingNumber}, পণ্য: {crop.NameBn}, দোকান: {complaint.ShopOrBusinessName}"
        };
        await _auditLogRepo.AddAsync(audit, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var resultDto = MapToDto(complaint, crop.NameBn);
        return Result<ConsumerComplaintDto>.Success(resultDto);
    }

    public async Task<Result<ComplaintTrackingResultDto>> TrackComplaintAsync(string trackingNumber, string complainantPhone, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber) || string.IsNullOrWhiteSpace(complainantPhone))
        {
            return Result<ComplaintTrackingResultDto>.Failure("ট্র্যাকিং নম্বর এবং দাখিলকৃত মোবাইল নম্বর উভয়ই আবশ্যক।");
        }

        var complaint = await _complaintRepo.GetByTrackingNumberAsync(trackingNumber.Trim(), cancellationToken);
        if (complaint == null)
        {
            return Result<ComplaintTrackingResultDto>.Failure("প্রদত্ত ট্র্যাকিং নম্বরে কোনো অভিযোগ খুঁজে পাওয়া যায়নি।");
        }

        // Verify phone for privacy protection
        var cleanProvided = NormalizePhone(complainantPhone);
        var cleanStored = NormalizePhone(complaint.ComplainantPhone);

        if (!string.Equals(cleanProvided, cleanStored, StringComparison.OrdinalIgnoreCase))
        {
            return Result<ComplaintTrackingResultDto>.Failure("নিরাপত্তার স্বার্থে সঠিক যাচাইকৃত মোবাইল নম্বর প্রদান করুন।");
        }

        var result = new ComplaintTrackingResultDto
        {
            TrackingNumber = complaint.TrackingNumber,
            MaskedComplainantName = MaskName(complaint.ComplainantName),
            ShopOrBusinessName = complaint.ShopOrBusinessName,
            District = complaint.District,
            Upazila = complaint.Upazila,
            SellerType = complaint.SellerType,
            CropNameBn = complaint.Crop?.NameBn ?? "অজানা পণ্য",
            QuantityPurchased = complaint.QuantityPurchased,
            ReportedUnit = complaint.ReportedUnit,
            ActualPriceCharged = complaint.ActualPriceCharged,
            NormalizedPricePerUnit = complaint.NormalizedPricePerUnit,
            StandardUnit = complaint.StandardUnit,
            PurchaseDateTime = complaint.PurchaseDateTime,
            Status = complaint.Status,
            StatusDescriptionBn = GetStatusDescription(complaint.Status),
            SubmissionDateUtc = complaint.CreatedAtUtc,
            PublicStatusMessage = complaint.DecisionReason,
            ComparisonAnalysisNotes = complaint.ComparisonAnalysisNotes
        };

        return Result<ComplaintTrackingResultDto>.Success(result);
    }

    public async Task<ConsumerComplaintDto?> GetComplaintDetailsForReviewAsync(Guid complaintId, CancellationToken cancellationToken = default)
    {
        var complaint = await _complaintRepo.GetWithDetailsAsync(complaintId, cancellationToken);
        if (complaint == null) return null;

        return MapToDto(complaint, complaint.Crop?.NameBn ?? string.Empty);
    }

    public async Task<PaginatedList<ConsumerComplaintDto>> SearchComplaintsForReviewAsync(ComplaintFilterDto filter, CancellationToken cancellationToken = default)
    {
        var paged = await _complaintRepo.SearchComplaintsAsync(filter, cancellationToken);

        var dtos = paged.Items.Select(c => MapToDto(c, c.Crop?.NameBn ?? string.Empty)).ToList();

        return new PaginatedList<ConsumerComplaintDto>(dtos, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }

    public async Task<Result> ReviewComplaintAsync(ReviewComplaintDto dto, string reviewerUserId, CancellationToken cancellationToken = default)
    {
        var complaint = await _complaintRepo.GetByIdAsync(dto.ComplaintId, cancellationToken);
        if (complaint == null)
            return Result.Failure("অভিযোগটি খুঁজে পাওয়া যায়নি।");

        complaint.Status = dto.NewStatus;
        complaint.AssignedReviewerUserId = reviewerUserId;
        complaint.ReviewerNotes = dto.ReviewerNotes;
        complaint.DecisionReason = dto.DecisionReason;
        complaint.ReviewedAtUtc = DateTime.UtcNow;
        complaint.IsPubliclyVisible = dto.IsPubliclyVisible;
        complaint.PublicSummary = dto.PublicSummary;

        await _complaintRepo.UpdateAsync(complaint, cancellationToken);

        var audit = new AuditLog
        {
            EntityName = nameof(ConsumerComplaint),
            EntityId = complaint.Id.ToString(),
            Action = "ComplaintReviewed",
            PerformedByUserId = Guid.TryParse(reviewerUserId, out var uid) ? uid : null,
            DetailsJson = $"অভিযোগ পর্যালোচনা সম্পন্ন: ট্র্যাকিং {complaint.TrackingNumber}, নতুন অবস্থা: {dto.NewStatus}"
        };
        await _auditLogRepo.AddAsync(audit, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static string GenerateNeutralAnalysis(
        decimal normalizedPrice,
        string baseUnit,
        decimal? avgMarketPrice,
        decimal? minMarketPrice,
        decimal? maxMarketPrice,
        decimal? variancePct,
        OfficialPriceDirective? directive,
        bool isOfficialPriceExceeded)
    {
        var sb = new StringBuilder();

        sb.Append($"অভিযুক্ত বিক্রয়মূল্য: {normalizedPrice:F2} টাকা/{baseUnit}। ");

        if (avgMarketPrice.HasValue)
        {
            sb.Append($"একই তারিখে সংশ্লিষ্ট এলাকায় পর্যবেক্ষণকৃত গড় বাজারদর ছিল {avgMarketPrice.Value:F2} টাকা/{baseUnit} (সর্বনিম্ন {minMarketPrice:F2} - সর্বোচ্চ {maxMarketPrice:F2} টাকা)। ");

            if (variancePct.HasValue)
            {
                if (variancePct.Value > 0)
                {
                    sb.Append($"পর্যবেক্ষিত গড় বাজারদরের চেয়ে {variancePct.Value:F1}% বেশি পরিলক্ষিত হয়েছে। ");
                }
                else if (variancePct.Value < 0)
                {
                    sb.Append($"পর্যবেক্ষিত গড় বাজারদরের চেয়ে {Math.Abs(variancePct.Value):F1}% কম পরিলক্ষিত হয়েছে। ");
                }
                else
                {
                    sb.Append("পর্যবেক্ষিত গড় বাজারদরের সাথে সামঞ্জস্যপূর্ণ। ");
                }
            }
        }
        else
        {
            sb.Append("উক্ত তারিখে ওই এলাকার নির্দিষ্ট পণ্যের কোনো আনুষ্ঠানিক বাজার পর্যবেক্ষণ তথ্য রেকর্ড করা নেই। ");
        }

        if (directive != null)
        {
            sb.Append($"আইনগত সরকারি মূল্য নির্দেশনা ({directive.DirectiveNumber}) অনুসারে সর্বোচ্চ নির্ধারিত মূল্য {directive.LegalMaxPrice:F2} টাকা/{directive.StandardUnit}। ");
            if (isOfficialPriceExceeded)
            {
                sb.Append("দাখিলকৃত মূল্য সরকারি সর্বোচ্চ আইনি নির্দেশনার চেয়ে বেশি দেখা যাচ্ছে; এটি ভোক্তা অধিকার বিধি অনুযায়ী আনুষ্ঠানিক পর্যালোচনার যোগ্য।");
            }
            else
            {
                sb.Append("দাখিলকৃত মূল্য সরকারি নির্ধারিত সর্বোচ্চ আইনি সীমার মধ্যেই রয়েছে।");
            }
        }
        else
        {
            sb.Append("উল্লেখ্য, কোনো পণ্যের মূল্য গড় বা সর্বোচ্চ বাজারদরের চেয়ে বেশি হলেই তা স্বয়ংক্রিয়ভাবে কোনো আইনগত লঙ্ঘন হিসেবে সাব্যস্ত হয় না, যদি না প্রযোজ্য কোনো সরকারি আইনি বাধ্যবাধকতা জারি থাকে।");
        }

        return sb.ToString();
    }

    private static ConsumerComplaintDto MapToDto(ConsumerComplaint c, string cropNameBn)
    {
        return new ConsumerComplaintDto
        {
            Id = c.Id,
            TrackingNumber = c.TrackingNumber,
            ComplainantUserId = c.ComplainantUserId,
            ComplainantName = c.ComplainantName,
            ComplainantPhone = c.ComplainantPhone,
            ComplainantEmail = c.ComplainantEmail,
            ShopOrBusinessName = c.ShopOrBusinessName,
            ShopAddress = c.ShopAddress,
            MarketId = c.MarketId,
            MarketNameBn = c.Market?.NameBn,
            District = c.District,
            Upazila = c.Upazila,
            SellerType = c.SellerType,
            CropId = c.CropId,
            CropNameBn = cropNameBn,
            QuantityPurchased = c.QuantityPurchased,
            ReportedUnit = c.ReportedUnit,
            ActualPriceCharged = c.ActualPriceCharged,
            NormalizedPricePerUnit = c.NormalizedPricePerUnit,
            StandardUnit = c.StandardUnit,
            PurchaseDateTime = c.PurchaseDateTime,
            Description = c.Description,
            HasReceiptEvidence = !string.IsNullOrWhiteSpace(c.ReceiptEvidencePath),
            ReceiptEvidencePath = c.ReceiptEvidencePath,
            ReceiptFileName = c.ReceiptFileName,
            Status = c.Status,
            AssignedReviewerUserId = c.AssignedReviewerUserId,
            ReviewerNotes = c.ReviewerNotes,
            DecisionReason = c.DecisionReason,
            ReviewedAtUtc = c.ReviewedAtUtc,
            MatchedMarketAveragePrice = c.MatchedMarketAveragePrice,
            MatchedMarketMinPrice = c.MatchedMarketMinPrice,
            MatchedMarketMaxPrice = c.MatchedMarketMaxPrice,
            PriceVariancePercentage = c.PriceVariancePercentage,
            OfficialPriceDirectiveId = c.OfficialPriceDirectiveId,
            OfficialLegalPrice = c.OfficialLegalPrice,
            IsOfficialPriceExceeded = c.IsOfficialPriceExceeded,
            ComparisonAnalysisNotes = c.ComparisonAnalysisNotes,
            IsPubliclyVisible = c.IsPubliclyVisible,
            PublicSummary = c.PublicSummary,
            CreatedAtUtc = c.CreatedAtUtc
        };
    }

    private static string NormalizePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("880")) digits = digits.Substring(2);
        return digits;
    }

    private static string MaskName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "নাম প্রকাশে অনিচ্ছুক";
        if (name.Length <= 2) return name[0] + "*";
        return name.Substring(0, 2) + new string('*', Math.Max(1, name.Length - 4)) + name[^1];
    }

    private static string GetStatusDescription(ComplaintStatus status) => status switch
    {
        ComplaintStatus.Submitted => "অভিযোগ দাখিল হয়েছে এবং প্রাথমিক যাচাইয়ের অপেক্ষায় আছে",
        ComplaintStatus.UnderReview => "কর্মকর্তা দ্বারা অভিযোগ ও প্রমাণের নিরপেক্ষ বিশ্লেষণ চলছে",
        ComplaintStatus.AwaitingInformation => "অভিযোগকারীর কাছ থেকে আরও তথ্য বা প্রমাণ চাওয়া হয়েছে",
        ComplaintStatus.Resolved => "অভিযোগটি নিষ্পত্তি করা হয়েছে",
        ComplaintStatus.Rejected => "পর্যালোচনা শেষে অভিযোগটি অগ্রহণযোগ্য হিসেবে চিহ্নিত হয়েছে",
        ComplaintStatus.ReferredToAuthority => "আইনগত ব্যবস্থা গ্রহণের জন্য ভোক্তা অধিকার সংরক্ষণ অধিদপ্তরে প্রেরণ করা হয়েছে",
        _ => "অপেক্ষমাণ"
    };

    private static string HashString(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
