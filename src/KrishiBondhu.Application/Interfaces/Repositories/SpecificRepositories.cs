using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KrishiBondhu.Application.Common.Models;
using KrishiBondhu.Domain.Entities;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Application.Interfaces.Repositories;

public interface IHarvestLotRepository : IRepository<HarvestLot>
{
    Task<HarvestLot?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HarvestLot>> GetByFarmerIdAsync(Guid farmerProfileId, CancellationToken cancellationToken = default);
    Task<PaginatedList<HarvestLot>> SearchLotsAsync(
        string? searchTerm,
        Guid? cropId,
        string? district,
        LotStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<string> GenerateNextLotNumberAsync(string districtCode, CancellationToken cancellationToken = default);
}

public interface IBuyerOfferRepository : IRepository<BuyerOffer>
{
    Task<BuyerOffer?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BuyerOffer>> GetOffersForLotAsync(Guid harvestLotId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BuyerOffer>> GetOffersByBuyerAsync(Guid buyerProfileId, CancellationToken cancellationToken = default);
    Task<string> GenerateNextOfferCodeAsync(CancellationToken cancellationToken = default);
    Task RejectOtherOffersForLotAsync(Guid harvestLotId, Guid acceptedOfferId, CancellationToken cancellationToken = default);
}

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetWithFullDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetOrdersForFarmerAsync(Guid farmerProfileId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetOrdersForBuyerAsync(Guid buyerProfileId, CancellationToken cancellationToken = default);
    Task<PaginatedList<Order>> SearchOrdersAsync(
        string? searchTerm,
        OrderStatus? status,
        Guid? farmerProfileId,
        Guid? buyerProfileId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<string> GenerateNextOrderNumberAsync(CancellationToken cancellationToken = default);
}

public interface IWeighingRecordRepository : IRepository<WeighingRecord>
{
    Task<IReadOnlyList<WeighingRecord>> GetRecordsForOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<WeighingRecord?> GetLatestConfirmedRecordAsync(Guid orderId, CancellationToken cancellationToken = default);
}

public interface IPaymentRecordRepository : IRepository<PaymentRecord>
{
    Task<IReadOnlyList<PaymentRecord>> GetPaymentsForOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalVerifiedAmountPaidForOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}

public interface IDisputeCaseRepository : IRepository<DisputeCase>
{
    Task<DisputeCase?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DisputeCase>> GetDisputesForOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<PaginatedList<DisputeCase>> GetDisputesAsync(DisputeStatus? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<string> GenerateNextDisputeNumberAsync(CancellationToken cancellationToken = default);
}

public interface ICropRepository : IRepository<Crop>
{
    Task<IReadOnlyList<Crop>> GetActiveCropsAsync(CancellationToken cancellationToken = default);
}

public interface IFarmerProfileRepository : IRepository<FarmerProfile>
{
    Task<FarmerProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FarmerProfile?> GetWithPlotsAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IBuyerProfileRepository : IRepository<BuyerProfile>
{
    Task<BuyerProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<BuyerProfile?> GetWithDemandsAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<PaginatedList<AuditLog>> GetAuditLogsAsync(string? entityName, string? entityId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}

public interface INotificationRepository : IRepository<Notification>
{
    Task<IReadOnlyList<Notification>> GetUnreadNotificationsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
}

public interface IMarketRepository : IRepository<Market>
{
    Task<IReadOnlyList<Market>> GetActiveMarketsAsync(string? district = null, CancellationToken cancellationToken = default);
    Task<Market?> GetByCodeAsync(string marketCode, CancellationToken cancellationToken = default);
}

public interface IPriceSourceRepository : IRepository<PriceSource>
{
    Task<IReadOnlyList<PriceSource>> GetActiveSourcesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PriceSource>> GetOfficialSourcesAsync(CancellationToken cancellationToken = default);
}

public interface IMarketPriceObservationRepository : IRepository<MarketPriceObservation>
{
    Task<PaginatedList<MarketPriceObservation>> GetObservationsAsync(DTOs.MarketPriceFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MarketPriceObservation>> GetObservationsForCropAndDateAsync(Guid cropId, DateTime date, MarketType? marketType = null, string? district = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MarketPriceObservation>> GetObservationsForCropAndDateRangeAsync(Guid cropId, DateTime fromDate, DateTime toDate, string? district = null, MarketType? marketType = null, CancellationToken cancellationToken = default);
    Task<MarketPriceObservation?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsDuplicateObservationAsync(Guid cropId, Guid marketId, MarketType marketType, DateTime observationDate, Guid priceSourceId, QualityGrade grade, CancellationToken cancellationToken = default);
}

public interface IOfficialPriceDirectiveRepository : IRepository<OfficialPriceDirective>
{
    Task<IReadOnlyList<OfficialPriceDirective>> GetActiveDirectivesForCropAsync(Guid cropId, DateTime date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OfficialPriceDirective>> GetDirectivesAsync(Guid? cropId = null, string? district = null, bool? activeOnly = true, CancellationToken cancellationToken = default);
    Task<OfficialPriceDirective?> GetByDirectiveNumberAsync(string directiveNumber, CancellationToken cancellationToken = default);
}

public interface IConsumerComplaintRepository : IRepository<ConsumerComplaint>
{
    Task<ConsumerComplaint?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default);
    Task<ConsumerComplaint?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedList<ConsumerComplaint>> SearchComplaintsAsync(DTOs.ComplaintFilterDto filter, CancellationToken cancellationToken = default);
    Task<string> GenerateNextTrackingNumberAsync(CancellationToken cancellationToken = default);
    Task<int> GetRecentSubmissionsCountByIpAsync(string clientIpHash, DateTime sinceUtc, CancellationToken cancellationToken = default);
}
