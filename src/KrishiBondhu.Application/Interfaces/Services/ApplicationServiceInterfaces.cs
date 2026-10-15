using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KrishiBondhu.Application.Common.Models;
using KrishiBondhu.Application.DTOs;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Application.Interfaces.Services;

public interface IAuthService
{
    Task<Result<UserSessionDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
    Task<Result<string>> GenerateSignInTokenAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
    Task<Result<string>> GenerateSignInTokenForUserAsync(Guid userId, bool isPersistent = false, CancellationToken cancellationToken = default);
    Task<Result<UserSessionDto>> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);
    Task<Result> SignOutAsync();
    Task<UserSessionDto?> GetCurrentUserSessionAsync(CancellationToken cancellationToken = default);
}

public interface IFarmerService
{
    Task<FarmerProfileDto?> GetProfileByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<FarmerProfileDto>> CreateOrUpdateProfileAsync(FarmerProfileDto profileDto, CancellationToken cancellationToken = default);
    Task<Result<FarmPlotDto>> AddPlotAsync(FarmPlotDto plotDto, CancellationToken cancellationToken = default);
    Task<Result> DeletePlotAsync(Guid plotId, CancellationToken cancellationToken = default);
}

public interface IBuyerService
{
    Task<BuyerProfileDto?> GetProfileByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<BuyerProfileDto>> CreateOrUpdateProfileAsync(BuyerProfileDto profileDto, CancellationToken cancellationToken = default);
    Task<PaginatedList<BuyerDemandDto>> GetDemandsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<BuyerDemandDto>> CreateDemandAsync(CreateBuyerDemandDto demandDto, CancellationToken cancellationToken = default);
}

public interface ICropService
{
    Task<IReadOnlyList<CropDto>> GetAllCropsAsync(CancellationToken cancellationToken = default);
    Task<CropDto?> GetCropByIdAsync(Guid cropId, CancellationToken cancellationToken = default);
}

public interface IHarvestLotService
{
    Task<Result<HarvestLotDto>> CreateLotAsync(CreateHarvestLotDto dto, CancellationToken cancellationToken = default);
    Task<HarvestLotDto?> GetLotByIdAsync(Guid lotId, CancellationToken cancellationToken = default);
    Task<PaginatedList<HarvestLotDto>> SearchLotsAsync(
        string? searchTerm,
        Guid? cropId,
        string? district,
        LotStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HarvestLotDto>> GetMyLotsAsync(CancellationToken cancellationToken = default);
    Task<Result> UpdateLotStatusAsync(Guid lotId, LotStatus status, CancellationToken cancellationToken = default);
}

public interface IOfferComparisonService
{
    Task<Result<BuyerOfferDto>> SubmitOfferAsync(CreateBuyerOfferDto dto, CancellationToken cancellationToken = default);
    Task<OfferComparisonSummaryDto> GetOfferComparisonForLotAsync(Guid harvestLotId, decimal? estimatedProductionCostPerKg = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BuyerOfferDto>> GetOffersByBuyerAsync(Guid buyerProfileId, CancellationToken cancellationToken = default);
    Task<BuyerOfferDto?> GetOfferByIdAsync(Guid offerId, CancellationToken cancellationToken = default);
}

public interface IOrderLifecycleService
{
    Task<Result<OrderDto>> AcceptOfferAndCreateOrderAsync(AcceptOfferDto dto, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<PaginatedList<OrderDto>> GetMyOrdersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Result> TransitionOrderStatusAsync(Guid orderId, OrderStatus newStatus, string? reason = null, CancellationToken cancellationToken = default);
}

public interface IWeighingService
{
    Task<Result<WeighingRecordDto>> RecordWeighingAsync(RecordWeighingDto dto, CancellationToken cancellationToken = default);
    Task<Result> ConfirmWeighingAsync(Guid weighingRecordId, bool isAgreed, string? notes = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WeighingRecordDto>> GetWeighingHistoryForOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}

public interface IPaymentService
{
    Task<Result<PaymentRecordDto>> RecordDirectPaymentAsync(RecordPaymentDto dto, CancellationToken cancellationToken = default);
    Task<Result> VerifyPaymentAsync(VerifyPaymentDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentRecordDto>> GetPaymentHistoryForOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}

public interface IDisputeService
{
    Task<Result<DisputeCaseDto>> RaiseDisputeAsync(RaiseDisputeDto dto, CancellationToken cancellationToken = default);
    Task<Result> ResolveDisputeAsync(ResolveDisputeDto dto, CancellationToken cancellationToken = default);
    Task<PaginatedList<DisputeCaseDto>> GetDisputesAsync(DisputeStatus? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<DisputeCaseDto?> GetDisputeByIdAsync(Guid disputeId, CancellationToken cancellationToken = default);
}

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetMyUnreadNotificationsAsync(CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task SendNotificationAsync(Guid userId, string titleBn, string titleEn, string messageBn, string messageEn, string type, string? url = null, CancellationToken cancellationToken = default);
}

public interface ILocalizationService
{
    string CurrentLanguage { get; }
    void SetLanguage(string languageCode);
    string Get(string key);
    string FormatCurrency(decimal amount);
    string FormatWeight(decimal weight, WeightUnit unit);
}

public interface IMarketPriceService
{
    Task<MarketPriceSummaryDto> GetMarketPriceSummaryAsync(Guid cropId, DateTime date, string? district = null, CancellationToken cancellationToken = default);
    Task<PaginatedList<MarketPriceObservationDto>> GetObservationsAsync(MarketPriceFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PriceTrendPointDto>> GetPriceTrendsAsync(Guid cropId, int days = 7, string? district = null, MarketType? marketType = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MarketDto>> GetActiveMarketsAsync(string? district = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PriceSourceDto>> GetActivePriceSourcesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OfficialPriceDirectiveDto>> GetActiveOfficialDirectivesAsync(Guid? cropId = null, string? district = null, CancellationToken cancellationToken = default);
    Task<Result<MarketPriceObservationDto>> RecordObservationAsync(CreateMarketPriceObservationDto dto, string submittedByUserId, CancellationToken cancellationToken = default);
    Task<Result> VerifyObservationAsync(Guid observationId, PriceVerificationStatus status, string verifiedByUserId, string? notes = null, CancellationToken cancellationToken = default);
}

public interface IConsumerComplaintService
{
    Task<Result<ConsumerComplaintDto>> SubmitComplaintAsync(SubmitConsumerComplaintDto dto, string? complainantUserId = null, string? clientIp = null, string? userAgent = null, CancellationToken cancellationToken = default);
    Task<Result<ComplaintTrackingResultDto>> TrackComplaintAsync(string trackingNumber, string complainantPhone, CancellationToken cancellationToken = default);
    Task<ConsumerComplaintDto?> GetComplaintDetailsForReviewAsync(Guid complaintId, CancellationToken cancellationToken = default);
    Task<PaginatedList<ConsumerComplaintDto>> SearchComplaintsForReviewAsync(ComplaintFilterDto filter, CancellationToken cancellationToken = default);
    Task<Result> ReviewComplaintAsync(ReviewComplaintDto dto, string reviewerUserId, CancellationToken cancellationToken = default);
}
