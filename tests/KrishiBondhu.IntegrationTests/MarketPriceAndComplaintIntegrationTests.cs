using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KrishiBondhu.Application.DTOs;
using KrishiBondhu.Domain.Entities;
using KrishiBondhu.Domain.Enums;
using KrishiBondhu.Infrastructure.Repositories;
using KrishiBondhu.Infrastructure.Services;
using Xunit;

namespace KrishiBondhu.IntegrationTests;

public class MarketPriceAndComplaintIntegrationTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly MarketPriceService _marketPriceService;
    private readonly ConsumerComplaintService _complaintService;
    private readonly MarketRepository _marketRepo;
    private readonly PriceSourceRepository _sourceRepo;
    private readonly MarketPriceObservationRepository _observationRepo;
    private readonly OfficialPriceDirectiveRepository _directiveRepo;
    private readonly ConsumerComplaintRepository _complaintRepo;
    private readonly CropRepository _cropRepo;
    private readonly AuditLogRepository _auditLogRepo;
    private readonly UnitOfWork _unitOfWork;

    private Market _testMarket = null!;
    private PriceSource _testSource = null!;
    private OfficialPriceDirective _testDirective = null!;

    public MarketPriceAndComplaintIntegrationTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        var context = _fixture.Context;

        _marketRepo = new MarketRepository(context);
        _sourceRepo = new PriceSourceRepository(context);
        _observationRepo = new MarketPriceObservationRepository(context);
        _directiveRepo = new OfficialPriceDirectiveRepository(context);
        _complaintRepo = new ConsumerComplaintRepository(context);
        _cropRepo = new CropRepository(context);
        _auditLogRepo = new AuditLogRepository(context);
        _unitOfWork = new UnitOfWork(context);

        _marketPriceService = new MarketPriceService(
            _observationRepo, _directiveRepo, _marketRepo, _sourceRepo, _cropRepo, _auditLogRepo, _unitOfWork);

        _complaintService = new ConsumerComplaintService(
            _complaintRepo, _observationRepo, _directiveRepo, _cropRepo, _marketRepo, _auditLogRepo, _unitOfWork);

        EnsureTestData();
    }

    private void EnsureTestData()
    {
        var existingMarket = _fixture.Context.Markets.FirstOrDefault(m => m.MarketCode == "MKT-TEST-001");
        if (existingMarket == null)
        {
            _testMarket = new Market
            {
                MarketCode = "MKT-TEST-001",
                NameBn = "টেস্ট পাইকারি আড়ত",
                NameEn = "Test Wholesale Market",
                District = "বগুড়া",
                Upazila = "শিবগঞ্জ",
                MarketType = MarketType.Both,
                IsActive = true
            };
            _fixture.Context.Markets.Add(_testMarket);
        }
        else
        {
            _testMarket = existingMarket;
        }

        var existingSource = _fixture.Context.PriceSources.FirstOrDefault(s => s.NameBn.Contains("টেস্ট"));
        if (existingSource == null)
        {
            _testSource = new PriceSource
            {
                NameBn = "টেস্ট জরিপকারী দল",
                NameEn = "Test Survey Team",
                SourceType = PriceSourceType.GovernmentOfficial,
                OrganizationName = "কৃষি বিপণন অধিদপ্তর",
                IsOfficialAuthority = true,
                IsActive = true
            };
            _fixture.Context.PriceSources.Add(_testSource);
        }
        else
        {
            _testSource = existingSource;
        }

        var existingDirective = _fixture.Context.OfficialPriceDirectives.FirstOrDefault(d => d.DirectiveNumber == "DIR-TEST-001");
        if (existingDirective == null)
        {
            _testDirective = new OfficialPriceDirective
            {
                DirectiveNumber = "DIR-TEST-001",
                TitleBn = "টেস্ট আইনি মূল্য নির্দেশিকা",
                IssuingAuthority = "জাতীয় ভোক্তা-অধিকার সংরক্ষণ অধিদপ্তর",
                CropId = _fixture.SeedCrop.Id,
                MarketType = MarketType.Both,
                ApplicableDistrict = null, // Nationwide
                EffectiveFromUtc = DateTime.UtcNow.AddDays(-10),
                EffectiveToUtc = DateTime.UtcNow.AddDays(10),
                OfficialBenchmarkPrice = 30.0m,
                LegalMaxPrice = 36.0m,
                StandardUnit = "কেজি",
                VerificationStatus = PriceVerificationStatus.Verified,
                IsActive = true
            };
            _fixture.Context.OfficialPriceDirectives.Add(_testDirective);
        }
        else
        {
            _testDirective = existingDirective;
        }

        _fixture.Context.SaveChanges();
    }

    [Fact]
    public async Task MarketPriceObservation_Recording_NormalizesUnitsAndDetectsDuplicates()
    {
        var targetDate = DateTime.Today;

        // 1. Record observation with Maund (1 Mon = 40 Kg at 1400 BDT => 35 BDT/Kg)
        var dto = new CreateMarketPriceObservationDto
        {
            CropId = _fixture.SeedCrop.Id,
            MarketId = _testMarket.Id,
            MarketType = MarketType.Wholesale,
            ObservationDate = targetDate,
            PriceSourceId = _testSource.Id,
            ObservedPrice = 1400m,
            ObservedQuantity = 1m,
            ObservedUnit = "মণ",
            Grade = QualityGrade.Standard
        };

        var result = await _marketPriceService.RecordObservationAsync(dto, "AdminUser");

        Assert.True(result.IsSuccess);
        Assert.Equal(35.00m, result.Value!.NormalizedPricePerUnit);
        Assert.Equal("কেজি", result.Value.NormalizedUnit);

        // 2. Duplicate detection check (same crop, market, date, source, and grade)
        var duplicateResult = await _marketPriceService.RecordObservationAsync(dto, "AdminUser");
        Assert.False(duplicateResult.IsSuccess);
        Assert.Contains("ইতিমধ্যে দাখিল করা হয়েছে", duplicateResult.ErrorMessage);
    }

    [Fact]
    public async Task MarketPriceSummary_CalculatesAccurateMetricsAndPreviousDayComparison()
    {
        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);

        // Seed yesterday wholesale observation: 32 BDT/Kg
        _fixture.Context.MarketPriceObservations.Add(new MarketPriceObservation
        {
            CropId = _fixture.SeedCrop.Id,
            MarketId = _testMarket.Id,
            MarketType = MarketType.Wholesale,
            ObservationDate = yesterday,
            PriceSourceId = _testSource.Id,
            ObservedPrice = 32m,
            ObservedQuantity = 1m,
            ObservedUnit = "কেজি",
            NormalizedPricePerUnit = 32m,
            NormalizedUnit = "কেজি",
            VerificationStatus = PriceVerificationStatus.Verified
        });

        // Seed today wholesale observations: 34 BDT/Kg and 36 BDT/Kg -> Avg: 35 BDT/Kg
        _fixture.Context.MarketPriceObservations.Add(new MarketPriceObservation
        {
            CropId = _fixture.SeedCrop.Id,
            MarketId = _testMarket.Id,
            MarketType = MarketType.Wholesale,
            ObservationDate = today,
            PriceSourceId = _testSource.Id,
            ObservedPrice = 34m,
            ObservedQuantity = 1m,
            ObservedUnit = "কেজি",
            NormalizedPricePerUnit = 34m,
            NormalizedUnit = "কেজি",
            Grade = QualityGrade.GradeA,
            VerificationStatus = PriceVerificationStatus.Verified
        });

        _fixture.Context.MarketPriceObservations.Add(new MarketPriceObservation
        {
            CropId = _fixture.SeedCrop.Id,
            MarketId = _testMarket.Id,
            MarketType = MarketType.Wholesale,
            ObservationDate = today,
            PriceSourceId = _testSource.Id,
            ObservedPrice = 36m,
            ObservedQuantity = 1m,
            ObservedUnit = "কেজি",
            NormalizedPricePerUnit = 36m,
            NormalizedUnit = "কেজি",
            Grade = QualityGrade.Premium,
            VerificationStatus = PriceVerificationStatus.Verified
        });

        await _fixture.Context.SaveChangesAsync();

        var summary = await _marketPriceService.GetMarketPriceSummaryAsync(_fixture.SeedCrop.Id, today, "বগুড়া");

        Assert.NotNull(summary);
        Assert.True(summary.Wholesale.ObservationCount >= 2);
        Assert.Equal(34m, summary.Wholesale.LowestPrice);
        Assert.Equal(36m, summary.Wholesale.HighestPrice);
        Assert.Equal(35m, summary.Wholesale.AveragePrice);

        // Previous Day Comparison
        Assert.True(summary.WholesaleComparison.HasHistoricalData);
        Assert.Equal(32m, summary.WholesaleComparison.PreviousDayAverage);
        Assert.Equal(3.0m, summary.WholesaleComparison.PriceDifference); // 35 - 32 = +3.00 BDT
    }

    [Fact]
    public async Task ConsumerComplaint_Submission_GeneratesTrackingAndNeutralComparison()
    {
        var complaintDto = new SubmitConsumerComplaintDto
        {
            ComplainantName = "মো. রাশেদুল ইসলাম",
            ComplainantPhone = "01733445566",
            ComplainantEmail = "rashed@test.com",
            ShopOrBusinessName = "জননী ট্রেডার্স",
            ShopAddress = "শিবগঞ্জ মোকাম রোড",
            District = "বগুড়া",
            Upazila = "শিবগঞ্জ",
            SellerType = MarketType.Retail,
            CropId = _fixture.SeedCrop.Id,
            QuantityPurchased = 10m,
            ReportedUnit = "কেজি",
            ActualPriceCharged = 450m, // 45 BDT/Kg (Directive limit is 36 BDT/Kg)
            PurchaseDateTime = DateTime.Now.AddHours(-2),
            Description = "নির্ধারিত মূল্যের চেয়ে বেশি নেওয়া হয়েছে।"
        };

        var result = await _complaintService.SubmitComplaintAsync(complaintDto, null, "192.168.1.100");

        Assert.True(result.IsSuccess);
        var complaint = result.Value!;

        Assert.StartsWith("CMP-", complaint.TrackingNumber);
        Assert.Equal(45.00m, complaint.NormalizedPricePerUnit);
        Assert.Equal(ComplaintStatus.Submitted, complaint.Status);
        Assert.False(complaint.IsPubliclyVisible); // Strictly non-public by default
        Assert.True(complaint.IsOfficialPriceExceeded);
        Assert.NotNull(complaint.ComparisonAnalysisNotes);
        Assert.Contains("আইনগত সরকারি মূল্য নির্দেশনা", complaint.ComparisonAnalysisNotes);
    }

    [Fact]
    public async Task ConsumerComplaint_Tracking_EnforcesPhoneVerification()
    {
        var complaintDto = new SubmitConsumerComplaintDto
        {
            ComplainantName = "আরিফুল হক",
            ComplainantPhone = "01844556677",
            ShopOrBusinessName = "মেসার্স সততা স্টোর",
            ShopAddress = "রাজাবাজার",
            District = "বগুড়া",
            Upazila = "বগুড়া সদর",
            SellerType = MarketType.Retail,
            CropId = _fixture.SeedCrop.Id,
            QuantityPurchased = 5m,
            ReportedUnit = "কেজি",
            ActualPriceCharged = 200m
        };

        var submitResult = await _complaintService.SubmitComplaintAsync(complaintDto, null, "192.168.1.101");
        Assert.True(submitResult.IsSuccess);
        var trackingNo = submitResult.Value!.TrackingNumber;

        // Correct Tracking with matching phone
        var trackSuccess = await _complaintService.TrackComplaintAsync(trackingNo, "01844556677");
        Assert.True(trackSuccess.IsSuccess);
        Assert.Contains("*", trackSuccess.Value!.MaskedComplainantName); // Name masked for privacy

        // Wrong Phone -> Must fail for privacy protection
        var trackFail = await _complaintService.TrackComplaintAsync(trackingNo, "01999999999");
        Assert.False(trackFail.IsSuccess);
        Assert.Contains("মোবাইল নম্বর", trackFail.ErrorMessage);
    }

    [Fact]
    public async Task ConsumerComplaint_ReviewWorkflow_TransitionsStatus_AndRecordsAudit()
    {
        var complaintDto = new SubmitConsumerComplaintDto
        {
            ComplainantName = "তাহমিনা আক্তার",
            ComplainantPhone = "01555667788",
            ShopOrBusinessName = "নিউ ভাই ভাই স্টোর",
            ShopAddress = "বটতলা",
            District = "বগুড়া",
            Upazila = "শিবগঞ্জ",
            SellerType = MarketType.Retail,
            CropId = _fixture.SeedCrop.Id,
            QuantityPurchased = 2m,
            ReportedUnit = "কেজি",
            ActualPriceCharged = 80m
        };

        var submitResult = await _complaintService.SubmitComplaintAsync(complaintDto, null, "192.168.1.102");
        Assert.True(submitResult.IsSuccess);
        var complaintId = submitResult.Value!.Id;

        // Perform Review by Authorized Personnel
        var reviewDto = new ReviewComplaintDto
        {
            ComplaintId = complaintId,
            NewStatus = ComplaintStatus.UnderReview,
            ReviewerNotes = "রশিদ যাচাইয়ের জন্য বিক্রেতার সাথে যোগাযোগ করা হচ্ছে।",
            DecisionReason = "অভিযোগটি বর্তমানে তদন্তকারী কর্মকর্তা পর্যালোচনা করছেন।",
            IsPubliclyVisible = false
        };

        var reviewResult = await _complaintService.ReviewComplaintAsync(reviewDto, "ReviewerOfficer01");
        Assert.True(reviewResult.IsSuccess);

        var updated = await _complaintRepo.GetByIdAsync(complaintId);
        Assert.NotNull(updated);
        Assert.Equal(ComplaintStatus.UnderReview, updated.Status);
        Assert.Equal("ReviewerOfficer01", updated.AssignedReviewerUserId);

        // Verify Audit Log
        var audit = await _fixture.Context.AuditLogs
            .FirstOrDefaultAsync(a => a.EntityName == nameof(ConsumerComplaint) && a.EntityId == complaintId.ToString() && a.Action == "ComplaintReviewed");
        Assert.NotNull(audit);
    }
}
