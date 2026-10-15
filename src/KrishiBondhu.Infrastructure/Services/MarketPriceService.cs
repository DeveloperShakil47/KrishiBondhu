using System;
using System.Collections.Generic;
using System.Linq;
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

public class MarketPriceService : IMarketPriceService
{
    private readonly IMarketPriceObservationRepository _observationRepo;
    private readonly IOfficialPriceDirectiveRepository _directiveRepo;
    private readonly IMarketRepository _marketRepo;
    private readonly IPriceSourceRepository _sourceRepo;
    private readonly ICropRepository _cropRepo;
    private readonly IAuditLogRepository _auditLogRepo;
    private readonly IUnitOfWork _unitOfWork;

    public MarketPriceService(
        IMarketPriceObservationRepository observationRepo,
        IOfficialPriceDirectiveRepository directiveRepo,
        IMarketRepository marketRepo,
        IPriceSourceRepository sourceRepo,
        ICropRepository cropRepo,
        IAuditLogRepository auditLogRepo,
        IUnitOfWork unitOfWork)
    {
        _observationRepo = observationRepo;
        _directiveRepo = directiveRepo;
        _marketRepo = marketRepo;
        _sourceRepo = sourceRepo;
        _cropRepo = cropRepo;
        _auditLogRepo = auditLogRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<MarketPriceSummaryDto> GetMarketPriceSummaryAsync(Guid cropId, DateTime date, string? district = null, CancellationToken cancellationToken = default)
    {
        var crop = await _cropRepo.GetByIdAsync(cropId, cancellationToken);
        var targetDate = date.Date;

        var summary = new MarketPriceSummaryDto
        {
            CropId = cropId,
            CropNameBn = crop?.NameBn ?? "অজানা পণ্য",
            CropCategory = crop?.Category ?? "সাধারণ",
            StandardUnit = crop?.StandardUnit ?? "কেজি",
            SummaryDate = targetDate
        };

        // 1. Observed today
        var observations = await _observationRepo.GetObservationsForCropAndDateAsync(cropId, targetDate, null, district, cancellationToken);

        var wholesaleObs = observations.Where(o => o.MarketType == MarketType.Wholesale || o.MarketType == MarketType.Both).ToList();
        var retailObs = observations.Where(o => o.MarketType == MarketType.Retail || o.MarketType == MarketType.Both).ToList();

        // Compute Wholesale Metrics
        if (wholesaleObs.Any())
        {
            summary.Wholesale = new DailyPriceMetricsDto
            {
                HighestPrice = wholesaleObs.Max(o => o.NormalizedPricePerUnit),
                LowestPrice = wholesaleObs.Min(o => o.NormalizedPricePerUnit),
                AveragePrice = Math.Round(wholesaleObs.Average(o => o.NormalizedPricePerUnit), 2),
                ObservationCount = wholesaleObs.Count
            };
        }

        // Compute Retail Metrics
        if (retailObs.Any())
        {
            summary.Retail = new DailyPriceMetricsDto
            {
                HighestPrice = retailObs.Max(o => o.NormalizedPricePerUnit),
                LowestPrice = retailObs.Min(o => o.NormalizedPricePerUnit),
                AveragePrice = Math.Round(retailObs.Average(o => o.NormalizedPricePerUnit), 2),
                ObservationCount = retailObs.Count
            };
        }

        // 2. Previous-day price comparison (Historical data only; do not invent)
        var prevDate = targetDate.AddDays(-1);
        var prevObservations = await _observationRepo.GetObservationsForCropAndDateAsync(cropId, prevDate, null, district, cancellationToken);

        var prevWholesale = prevObservations.Where(o => o.MarketType == MarketType.Wholesale || o.MarketType == MarketType.Both).ToList();
        var prevRetail = prevObservations.Where(o => o.MarketType == MarketType.Retail || o.MarketType == MarketType.Both).ToList();

        if (summary.Wholesale.ObservationCount > 0 && prevWholesale.Any())
        {
            var prevAvg = Math.Round(prevWholesale.Average(o => o.NormalizedPricePerUnit), 2);
            var diff = summary.Wholesale.AveragePrice - prevAvg;
            var pct = prevAvg > 0 ? Math.Round((diff / prevAvg) * 100m, 2) : 0m;

            summary.WholesaleComparison = new PriceComparisonDeltaDto
            {
                CurrentAverage = summary.Wholesale.AveragePrice,
                PreviousDayAverage = prevAvg,
                PriceDifference = diff,
                PercentageChange = pct,
                HasHistoricalData = true
            };
        }
        else
        {
            summary.WholesaleComparison = new PriceComparisonDeltaDto
            {
                CurrentAverage = summary.Wholesale.AveragePrice,
                HasHistoricalData = false
            };
        }

        if (summary.Retail.ObservationCount > 0 && prevRetail.Any())
        {
            var prevAvg = Math.Round(prevRetail.Average(o => o.NormalizedPricePerUnit), 2);
            var diff = summary.Retail.AveragePrice - prevAvg;
            var pct = prevAvg > 0 ? Math.Round((diff / prevAvg) * 100m, 2) : 0m;

            summary.RetailComparison = new PriceComparisonDeltaDto
            {
                CurrentAverage = summary.Retail.AveragePrice,
                PreviousDayAverage = prevAvg,
                PriceDifference = diff,
                PercentageChange = pct,
                HasHistoricalData = true
            };
        }
        else
        {
            summary.RetailComparison = new PriceComparisonDeltaDto
            {
                CurrentAverage = summary.Retail.AveragePrice,
                HasHistoricalData = false
            };
        }

        // 3. Officially published government price directives
        var directives = await _directiveRepo.GetActiveDirectivesForCropAsync(cropId, targetDate, cancellationToken);
        summary.ApplicableOfficialDirectives = directives
            .Where(d => string.IsNullOrWhiteSpace(d.ApplicableDistrict) || string.IsNullOrWhiteSpace(district) || d.ApplicableDistrict == district)
            .Select(d => new OfficialPriceDirectiveDto
            {
                Id = d.Id,
                DirectiveNumber = d.DirectiveNumber,
                TitleBn = d.TitleBn,
                IssuingAuthority = d.IssuingAuthority,
                SourceReference = d.SourceReference,
                CropId = d.CropId,
                CropNameBn = crop?.NameBn ?? string.Empty,
                MarketType = d.MarketType,
                ApplicableDistrict = d.ApplicableDistrict,
                ApplicableUpazila = d.ApplicableUpazila,
                EffectiveFromUtc = d.EffectiveFromUtc,
                EffectiveToUtc = d.EffectiveToUtc,
                OfficialBenchmarkPrice = d.OfficialBenchmarkPrice,
                LegalMaxPrice = d.LegalMaxPrice,
                StandardUnit = d.StandardUnit,
                ApplicableConditions = d.ApplicableConditions,
                VerificationStatus = d.VerificationStatus,
                IsActive = d.IsActive
            }).ToList();

        return summary;
    }

    public async Task<PaginatedList<MarketPriceObservationDto>> GetObservationsAsync(MarketPriceFilterDto filter, CancellationToken cancellationToken = default)
    {
        var paged = await _observationRepo.GetObservationsAsync(filter, cancellationToken);

        var dtos = paged.Items.Select(o => new MarketPriceObservationDto
        {
            Id = o.Id,
            CropId = o.CropId,
            CropNameBn = o.Crop?.NameBn ?? string.Empty,
            CropCategory = o.Crop?.Category ?? string.Empty,
            MarketId = o.MarketId,
            MarketNameBn = o.Market?.NameBn ?? string.Empty,
            District = o.Market?.District ?? string.Empty,
            Upazila = o.Market?.Upazila ?? string.Empty,
            MarketType = o.MarketType,
            ObservationDate = o.ObservationDate,
            PriceSourceId = o.PriceSourceId,
            SourceNameBn = o.PriceSource?.NameBn ?? string.Empty,
            ObservedPrice = o.ObservedPrice,
            ObservedQuantity = o.ObservedQuantity,
            ObservedUnit = o.ObservedUnit,
            NormalizedPricePerUnit = o.NormalizedPricePerUnit,
            NormalizedUnit = o.NormalizedUnit,
            MinPrice = o.MinPrice,
            MaxPrice = o.MaxPrice,
            Grade = o.Grade,
            VerificationStatus = o.VerificationStatus,
            Notes = o.Notes,
            CreatedAtUtc = o.CreatedAtUtc
        }).ToList();

        return new PaginatedList<MarketPriceObservationDto>(dtos, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }

    public async Task<IReadOnlyList<PriceTrendPointDto>> GetPriceTrendsAsync(Guid cropId, int days = 7, string? district = null, MarketType? marketType = null, CancellationToken cancellationToken = default)
    {
        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-Math.Abs(days));

        var observations = await _observationRepo.GetObservationsForCropAndDateRangeAsync(cropId, startDate, endDate, district, marketType, cancellationToken);

        // Group by Date using only real observations
        var trends = observations
            .GroupBy(o => o.ObservationDate.Date)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var wholesale = g.Where(x => x.MarketType == MarketType.Wholesale || x.MarketType == MarketType.Both).ToList();
                var retail = g.Where(x => x.MarketType == MarketType.Retail || x.MarketType == MarketType.Both).ToList();

                return new PriceTrendPointDto
                {
                    Date = g.Key,
                    FormattedDate = g.Key.ToString("dd MMM yyyy"),
                    AverageWholesalePrice = wholesale.Any() ? Math.Round(wholesale.Average(w => w.NormalizedPricePerUnit), 2) : null,
                    AverageRetailPrice = retail.Any() ? Math.Round(retail.Average(r => r.NormalizedPricePerUnit), 2) : null,
                    LowestObservedPrice = g.Min(x => x.NormalizedPricePerUnit),
                    HighestObservedPrice = g.Max(x => x.NormalizedPricePerUnit),
                    TotalObservations = g.Count()
                };
            })
            .ToList();

        return trends;
    }

    public async Task<IReadOnlyList<MarketDto>> GetActiveMarketsAsync(string? district = null, CancellationToken cancellationToken = default)
    {
        var markets = await _marketRepo.GetActiveMarketsAsync(district, cancellationToken);
        return markets.Select(m => new MarketDto
        {
            Id = m.Id,
            MarketCode = m.MarketCode,
            NameBn = m.NameBn,
            NameEn = m.NameEn,
            MarketType = m.MarketType,
            District = m.District,
            Upazila = m.Upazila,
            AddressDetails = m.AddressDetails,
            IsActive = m.IsActive
        }).ToList();
    }

    public async Task<IReadOnlyList<PriceSourceDto>> GetActivePriceSourcesAsync(CancellationToken cancellationToken = default)
    {
        var sources = await _sourceRepo.GetActiveSourcesAsync(cancellationToken);
        return sources.Select(s => new PriceSourceDto
        {
            Id = s.Id,
            NameBn = s.NameBn,
            NameEn = s.NameEn,
            SourceType = s.SourceType,
            OrganizationName = s.OrganizationName,
            IsOfficialAuthority = s.IsOfficialAuthority,
            Description = s.Description
        }).ToList();
    }

    public async Task<IReadOnlyList<OfficialPriceDirectiveDto>> GetActiveOfficialDirectivesAsync(Guid? cropId = null, string? district = null, CancellationToken cancellationToken = default)
    {
        var directives = await _directiveRepo.GetDirectivesAsync(cropId, district, activeOnly: true, cancellationToken);
        return directives.Select(d => new OfficialPriceDirectiveDto
        {
            Id = d.Id,
            DirectiveNumber = d.DirectiveNumber,
            TitleBn = d.TitleBn,
            IssuingAuthority = d.IssuingAuthority,
            SourceReference = d.SourceReference,
            CropId = d.CropId,
            CropNameBn = d.Crop?.NameBn ?? string.Empty,
            MarketType = d.MarketType,
            ApplicableDistrict = d.ApplicableDistrict,
            ApplicableUpazila = d.ApplicableUpazila,
            EffectiveFromUtc = d.EffectiveFromUtc,
            EffectiveToUtc = d.EffectiveToUtc,
            OfficialBenchmarkPrice = d.OfficialBenchmarkPrice,
            LegalMaxPrice = d.LegalMaxPrice,
            StandardUnit = d.StandardUnit,
            ApplicableConditions = d.ApplicableConditions,
            VerificationStatus = d.VerificationStatus,
            IsActive = d.IsActive
        }).ToList();
    }

    public async Task<Result<MarketPriceObservationDto>> RecordObservationAsync(CreateMarketPriceObservationDto dto, string submittedByUserId, CancellationToken cancellationToken = default)
    {
        // 1. Validation
        if (dto.ObservedPrice <= 0)
            return Result<MarketPriceObservationDto>.Failure("পর্যবেক্ষিত মূল্য শূন্যের বেশি হতে হবে।");

        if (dto.ObservedQuantity <= 0)
            return Result<MarketPriceObservationDto>.Failure("পর্যবেক্ষিত পরিমাণ শূন্যের বেশি হতে হবে।");

        var crop = await _cropRepo.GetByIdAsync(dto.CropId, cancellationToken);
        if (crop == null)
            return Result<MarketPriceObservationDto>.Failure("নির্বাচিত ফসল খুঁজে পাওয়া যায়নি।");

        var market = await _marketRepo.GetByIdAsync(dto.MarketId, cancellationToken);
        if (market == null)
            return Result<MarketPriceObservationDto>.Failure("নির্বাচিত বাজার খুঁজে পাওয়া যায়নি।");

        var source = await _sourceRepo.GetByIdAsync(dto.PriceSourceId, cancellationToken);
        if (source == null)
            return Result<MarketPriceObservationDto>.Failure("নির্বাচিত তথ্যের উৎস খুঁজে পাওয়া যায়নি।");

        // 2. Unit normalization check
        if (!UnitNormalizer.AreCompatible(dto.ObservedUnit, crop.StandardUnit))
        {
            return Result<MarketPriceObservationDto>.Failure($"পরিমাপের একক অসামঞ্জস্যপূর্ণ। ফসলের অনুমোদিত একক ধরন: {crop.StandardUnit}।");
        }

        var (normalizedPrice, baseUnit) = UnitNormalizer.CalculateNormalizedPrice(dto.ObservedPrice, dto.ObservedQuantity, dto.ObservedUnit);

        // 3. Duplicate detection
        var isDuplicate = await _observationRepo.ExistsDuplicateObservationAsync(
            dto.CropId, dto.MarketId, dto.MarketType, dto.ObservationDate, dto.PriceSourceId, dto.Grade, cancellationToken);

        if (isDuplicate)
        {
            return Result<MarketPriceObservationDto>.Failure("এই বাজার, ফসল, তারিখ এবং তথ্যের উৎসের জন্য উক্ত গ্রেডের মূল্য পর্যবেক্ষণ ইতিমধ্যে দাখিল করা হয়েছে। পূর্বের রেকর্ড অপরিবর্তিত রাখা হয়েছে।");
        }

        // 4. Calculate Min/Max Normalized if provided
        decimal? normMin = null;
        decimal? normMax = null;
        if (dto.MinPrice.HasValue && dto.MinPrice.Value > 0)
        {
            var (minNorm, _) = UnitNormalizer.CalculateNormalizedPrice(dto.MinPrice.Value, dto.ObservedQuantity, dto.ObservedUnit);
            normMin = minNorm;
        }
        if (dto.MaxPrice.HasValue && dto.MaxPrice.Value > 0)
        {
            var (maxNorm, _) = UnitNormalizer.CalculateNormalizedPrice(dto.MaxPrice.Value, dto.ObservedQuantity, dto.ObservedUnit);
            normMax = maxNorm;
        }

        // 5. Build entity
        var observation = new MarketPriceObservation
        {
            CropId = dto.CropId,
            MarketId = dto.MarketId,
            MarketType = dto.MarketType,
            ObservationDate = DateTime.SpecifyKind(dto.ObservationDate.Date, DateTimeKind.Utc),
            PriceSourceId = dto.PriceSourceId,
            ObservedPrice = dto.ObservedPrice,
            ObservedQuantity = dto.ObservedQuantity,
            ObservedUnit = UnitNormalizer.CanonicalizeUnit(dto.ObservedUnit),
            NormalizedPricePerUnit = normalizedPrice,
            NormalizedUnit = baseUnit,
            MinPrice = dto.MinPrice,
            MaxPrice = dto.MaxPrice,
            NormalizedMinPrice = normMin,
            NormalizedMaxPrice = normMax,
            Grade = dto.Grade,
            Notes = dto.Notes,
            VerificationStatus = source.IsOfficialAuthority ? PriceVerificationStatus.Verified : PriceVerificationStatus.Unverified,
            CreatedBy = submittedByUserId
        };

        if (source.IsOfficialAuthority)
        {
            observation.VerifiedByUserId = submittedByUserId;
            observation.VerifiedAtUtc = DateTime.UtcNow;
        }

        await _observationRepo.AddAsync(observation, cancellationToken);

        // 6. Audit log
        var audit = new AuditLog
        {
            EntityName = nameof(MarketPriceObservation),
            EntityId = observation.Id.ToString(),
            Action = "ObservationRecorded",
            PerformedByUserId = Guid.TryParse(submittedByUserId, out var uid) ? uid : null,
            DetailsJson = $"মূল্য পর্যবেক্ষণ রেকর্ড: {crop.NameBn}, বাজার: {market.NameBn}, মূল্য: {normalizedPrice:F2} টাকা/{baseUnit}"
        };
        await _auditLogRepo.AddAsync(audit, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var resultDto = new MarketPriceObservationDto
        {
            Id = observation.Id,
            CropId = observation.CropId,
            CropNameBn = crop.NameBn,
            CropCategory = crop.Category,
            MarketId = observation.MarketId,
            MarketNameBn = market.NameBn,
            District = market.District,
            Upazila = market.Upazila,
            MarketType = observation.MarketType,
            ObservationDate = observation.ObservationDate,
            PriceSourceId = observation.PriceSourceId,
            SourceNameBn = source.NameBn,
            ObservedPrice = observation.ObservedPrice,
            ObservedQuantity = observation.ObservedQuantity,
            ObservedUnit = observation.ObservedUnit,
            NormalizedPricePerUnit = observation.NormalizedPricePerUnit,
            NormalizedUnit = observation.NormalizedUnit,
            MinPrice = observation.MinPrice,
            MaxPrice = observation.MaxPrice,
            Grade = observation.Grade,
            VerificationStatus = observation.VerificationStatus,
            Notes = observation.Notes,
            CreatedAtUtc = observation.CreatedAtUtc
        };

        return Result<MarketPriceObservationDto>.Success(resultDto);
    }

    public async Task<Result> VerifyObservationAsync(Guid observationId, PriceVerificationStatus status, string verifiedByUserId, string? notes = null, CancellationToken cancellationToken = default)
    {
        var observation = await _observationRepo.GetByIdAsync(observationId, cancellationToken);
        if (observation == null)
            return Result.Failure("মূল্য পর্যবেক্ষণ রেকর্ড পাওয়া যায়নি।");

        observation.VerificationStatus = status;
        observation.VerifiedByUserId = verifiedByUserId;
        observation.VerifiedAtUtc = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(notes))
        {
            observation.Notes = string.IsNullOrWhiteSpace(observation.Notes) ? notes : $"{observation.Notes} | যাচাই নোট: {notes}";
        }

        await _observationRepo.UpdateAsync(observation, cancellationToken);

        var audit = new AuditLog
        {
            EntityName = nameof(MarketPriceObservation),
            EntityId = observation.Id.ToString(),
            Action = "ObservationVerified",
            PerformedByUserId = Guid.TryParse(verifiedByUserId, out var uid) ? uid : null,
            DetailsJson = $"মূল্য যাচাই স্ট্যাটাস পরিবর্তন: {status}"
        };
        await _auditLogRepo.AddAsync(audit, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<MarketDto>> CreateMarketAsync(CreateMarketDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.NameBn))
            return Result<MarketDto>.Failure("বাজারের বাংলা নাম আবশ্যক।");

        if (string.IsNullOrWhiteSpace(dto.District))
            return Result<MarketDto>.Failure("জেলা নির্বাচন আবশ্যক।");

        if (string.IsNullOrWhiteSpace(dto.Upazila))
            return Result<MarketDto>.Failure("উপজেলা নির্বাচন আবশ্যক।");

        var code = "MKT-" + Guid.NewGuid().ToString("N")[..6].ToUpper();

        var market = new Market
        {
            MarketCode = code,
            NameBn = dto.NameBn.Trim(),
            NameEn = string.IsNullOrWhiteSpace(dto.NameEn) ? dto.NameBn.Trim() : dto.NameEn.Trim(),
            District = dto.District.Trim(),
            Upazila = dto.Upazila.Trim(),
            MarketType = dto.MarketType,
            AddressDetails = dto.AddressDetails?.Trim(),
            IsActive = true
        };

        await _marketRepo.AddAsync(market, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<MarketDto>.Success(new MarketDto
        {
            Id = market.Id,
            MarketCode = market.MarketCode,
            NameBn = market.NameBn,
            NameEn = market.NameEn,
            MarketType = market.MarketType,
            District = market.District,
            Upazila = market.Upazila,
            AddressDetails = market.AddressDetails,
            IsActive = market.IsActive
        });
    }
}
