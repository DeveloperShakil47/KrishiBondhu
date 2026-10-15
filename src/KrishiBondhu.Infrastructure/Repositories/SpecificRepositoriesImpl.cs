using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KrishiBondhu.Application.Common.Models;
using KrishiBondhu.Application.Interfaces.Repositories;
using KrishiBondhu.Domain.Entities;
using KrishiBondhu.Domain.Enums;
using KrishiBondhu.Infrastructure.Data;

namespace KrishiBondhu.Infrastructure.Repositories;

public class HarvestLotRepository : GenericRepository<HarvestLot>, IHarvestLotRepository
{
    public HarvestLotRepository(KrishiBondhuDbContext context) : base(context) { }

    public async Task<HarvestLot?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(h => h.Crop)
            .Include(h => h.FarmerProfile)
            .Include(h => h.Offers.Where(o => !o.IsDeleted))
                .ThenInclude(o => o.BuyerProfile)
            .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<HarvestLot>> GetByFarmerIdAsync(Guid farmerProfileId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(h => h.Crop)
            .Include(h => h.Offers.Where(o => !o.IsDeleted))
            .Where(h => h.FarmerProfileId == farmerProfileId && !h.IsDeleted)
            .OrderByDescending(h => h.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaginatedList<HarvestLot>> SearchLotsAsync(
        string? searchTerm,
        Guid? cropId,
        string? district,
        LotStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(h => h.Crop)
            .Include(h => h.FarmerProfile)
            .Include(h => h.Offers.Where(o => !o.IsDeleted))
            .Where(h => !h.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(h =>
                h.LotNumber.ToLower().Contains(term) ||
                (h.Crop != null && (h.Crop.NameBn.ToLower().Contains(term) || h.Crop.NameEn.ToLower().Contains(term))) ||
                h.PickupDistrict.ToLower().Contains(term) ||
                h.PickupUpazila.ToLower().Contains(term));
        }

        if (cropId.HasValue && cropId.Value != Guid.Empty)
        {
            query = query.Where(h => h.CropId == cropId.Value);
        }

        if (!string.IsNullOrWhiteSpace(district))
        {
            query = query.Where(h => h.PickupDistrict == district);
        }

        if (status.HasValue)
        {
            query = query.Where(h => h.Status == status.Value);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(h => h.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<HarvestLot>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<string> GenerateNextLotNumberAsync(string districtCode, CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"LOT-{year}-{districtCode.ToUpper()}-";
        var count = await _dbSet.CountAsync(h => h.LotNumber.StartsWith(prefix), cancellationToken);
        return $"{prefix}{(count + 1):D4}";
    }
}

public class BuyerOfferRepository : GenericRepository<BuyerOffer>, IBuyerOfferRepository
{
    public BuyerOfferRepository(KrishiBondhuDbContext context) : base(context) { }

    public async Task<BuyerOffer?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.BuyerProfile)
            .Include(o => o.HarvestLot)
                .ThenInclude(h => h!.Crop)
            .Include(o => o.HarvestLot)
                .ThenInclude(h => h!.FarmerProfile)
            .Include(o => o.ItemizedCosts)
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<BuyerOffer>> GetOffersForLotAsync(Guid harvestLotId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(o => o.BuyerProfile)
            .Include(o => o.ItemizedCosts)
            .Where(o => o.HarvestLotId == harvestLotId && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BuyerOffer>> GetOffersByBuyerAsync(Guid buyerProfileId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(o => o.HarvestLot)
                .ThenInclude(h => h!.Crop)
            .Include(o => o.HarvestLot)
                .ThenInclude(h => h!.FarmerProfile)
            .Where(o => o.BuyerProfileId == buyerProfileId && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<string> GenerateNextOfferCodeAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"OFR-{year}-";
        var count = await _dbSet.CountAsync(o => o.OfferCode.StartsWith(prefix), cancellationToken);
        return $"{prefix}{(count + 1):D4}";
    }

    public async Task RejectOtherOffersForLotAsync(Guid harvestLotId, Guid acceptedOfferId, CancellationToken cancellationToken = default)
    {
        var pendingOffers = await _dbSet
            .Where(o => o.HarvestLotId == harvestLotId && o.Id != acceptedOfferId && (o.Status == OfferStatus.Submitted || o.Status == OfferStatus.UnderReview) && !o.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var o in pendingOffers)
        {
            o.Status = OfferStatus.Rejected;
            o.RespondedAtUtc = DateTime.UtcNow;
            o.FarmerResponseNote = "অন্য একটি দরপ্রস্তাব গৃহীত হয়েছে।";
        }
    }
}

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(KrishiBondhuDbContext context) : base(context) { }

    public async Task<Order?> GetWithFullDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.HarvestLot)
                .ThenInclude(h => h!.Crop)
            .Include(o => o.FarmerProfile)
            .Include(o => o.BuyerProfile)
            .Include(o => o.WeighingRecords.Where(w => !w.IsDeleted))
            .Include(o => o.PaymentRecords.Where(p => !p.IsDeleted))
            .Include(o => o.DeliveryRecords.Where(d => !d.IsDeleted))
            .Include(o => o.Disputes.Where(d => !d.IsDeleted))
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted, cancellationToken);
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.HarvestLot)
                .ThenInclude(h => h!.Crop)
            .Include(o => o.FarmerProfile)
            .Include(o => o.BuyerProfile)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && !o.IsDeleted, cancellationToken);
    }

    public async Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(o => o.IdempotencyKey == idempotencyKey && !o.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetOrdersForFarmerAsync(Guid farmerProfileId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(o => o.HarvestLot)
                .ThenInclude(h => h!.Crop)
            .Include(o => o.BuyerProfile)
            .Include(o => o.WeighingRecords)
            .Include(o => o.PaymentRecords)
            .Where(o => o.FarmerProfileId == farmerProfileId && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetOrdersForBuyerAsync(Guid buyerProfileId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(o => o.HarvestLot)
                .ThenInclude(h => h!.Crop)
            .Include(o => o.FarmerProfile)
            .Include(o => o.WeighingRecords)
            .Include(o => o.PaymentRecords)
            .Where(o => o.BuyerProfileId == buyerProfileId && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaginatedList<Order>> SearchOrdersAsync(
        string? searchTerm,
        OrderStatus? status,
        Guid? farmerProfileId,
        Guid? buyerProfileId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(o => o.HarvestLot)
                .ThenInclude(h => h!.Crop)
            .Include(o => o.FarmerProfile)
            .Include(o => o.BuyerProfile)
            .Where(o => !o.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(o =>
                o.OrderNumber.ToLower().Contains(term) ||
                (o.FarmerProfile != null && o.FarmerProfile.FullNameBn.ToLower().Contains(term)) ||
                (o.BuyerProfile != null && o.BuyerProfile.BusinessName.ToLower().Contains(term)));
        }

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (farmerProfileId.HasValue)
        {
            query = query.Where(o => o.FarmerProfileId == farmerProfileId.Value);
        }

        if (buyerProfileId.HasValue)
        {
            query = query.Where(o => o.BuyerProfileId == buyerProfileId.Value);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<Order>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<string> GenerateNextOrderNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"KB-{year}-";
        var count = await _dbSet.CountAsync(o => o.OrderNumber.StartsWith(prefix), cancellationToken);
        return $"{prefix}{(count + 1):D5}";
    }
}

public class WeighingRecordRepository : GenericRepository<WeighingRecord>, IWeighingRecordRepository
{
    public WeighingRecordRepository(KrishiBondhuDbContext context) : base(context) { }

    public async Task<IReadOnlyList<WeighingRecord>> GetRecordsForOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(w => w.OrderId == orderId && !w.IsDeleted)
            .OrderBy(w => w.SequenceNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<WeighingRecord?> GetLatestConfirmedRecordAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(w => w.OrderId == orderId && w.Status == WeighingStatus.Confirmed && !w.IsDeleted)
            .OrderByDescending(w => w.SequenceNumber)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public class PaymentRecordRepository : GenericRepository<PaymentRecord>, IPaymentRecordRepository
{
    public PaymentRecordRepository(KrishiBondhuDbContext context) : base(context) { }

    public async Task<IReadOnlyList<PaymentRecord>> GetPaymentsForOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.OrderId == orderId && !p.IsDeleted)
            .OrderByDescending(p => p.PaymentDateUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalVerifiedAmountPaidForOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.OrderId == orderId && p.Status == PaymentStatus.Paid && !p.IsDeleted)
            .SumAsync(p => p.Amount, cancellationToken);
    }
}

public class DisputeCaseRepository : GenericRepository<DisputeCase>, IDisputeCaseRepository
{
    public DisputeCaseRepository(KrishiBondhuDbContext context) : base(context) { }

    public async Task<DisputeCase?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Order)
                .ThenInclude(o => o!.HarvestLot)
                    .ThenInclude(h => h!.Crop)
            .Include(d => d.Order)
                .ThenInclude(o => o!.FarmerProfile)
            .Include(d => d.Order)
                .ThenInclude(o => o!.BuyerProfile)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<DisputeCase>> GetDisputesForOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(d => d.OrderId == orderId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaginatedList<DisputeCase>> GetDisputesAsync(DisputeStatus? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(d => d.Order)
                .ThenInclude(o => o!.HarvestLot)
                    .ThenInclude(h => h!.Crop)
            .Where(d => !d.IsDeleted);

        if (status.HasValue)
        {
            query = query.Where(d => d.Status == status.Value);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(d => d.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<DisputeCase>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<string> GenerateNextDisputeNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"DSP-{year}-";
        var count = await _dbSet.CountAsync(d => d.DisputeNumber.StartsWith(prefix), cancellationToken);
        return $"{prefix}{(count + 1):D4}";
    }
}

public class CropRepository : GenericRepository<Crop>, ICropRepository
{
    public CropRepository(KrishiBondhuDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Crop>> GetActiveCropsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.NameBn)
            .ToListAsync(cancellationToken);
    }
}

public class FarmerProfileRepository : GenericRepository<FarmerProfile>, IFarmerProfileRepository
{
    public FarmerProfileRepository(KrishiBondhuDbContext context) : base(context) { }

    public async Task<FarmerProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Plots.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(f => f.UserId == userId && !f.IsDeleted, cancellationToken);
    }

    public async Task<FarmerProfile?> GetWithPlotsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Plots.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, cancellationToken);
    }
}

public class BuyerProfileRepository : GenericRepository<BuyerProfile>, IBuyerProfileRepository
{
    public BuyerProfileRepository(KrishiBondhuDbContext context) : base(context) { }

    public async Task<BuyerProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.Demands.Where(d => !d.IsDeleted))
            .FirstOrDefaultAsync(b => b.UserId == userId && !b.IsDeleted, cancellationToken);
    }

    public async Task<BuyerProfile?> GetWithDemandsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.Demands.Where(d => !d.IsDeleted))
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);
    }
}

public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(KrishiBondhuDbContext context) : base(context) { }

    public async Task<PaginatedList<AuditLog>> GetAuditLogsAsync(string? entityName, string? entityId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().Where(a => !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            query = query.Where(a => a.EntityName == entityName);
        }

        if (!string.IsNullOrWhiteSpace(entityId))
        {
            query = query.Where(a => a.EntityId == entityId);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<AuditLog>(items, totalCount, pageNumber, pageSize);
    }
}

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(KrishiBondhuDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Notification>> GetUnreadNotificationsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(20)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _dbSet.FirstOrDefaultAsync(n => n.Id == notificationId, cancellationToken);
        if (notification != null)
        {
            notification.IsRead = true;
            notification.ReadAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

public class MarketRepository : GenericRepository<Market>, IMarketRepository
{
    public MarketRepository(KrishiBondhuDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Market>> GetActiveMarketsAsync(string? district = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().Where(m => m.IsActive && !m.IsDeleted);
        if (!string.IsNullOrWhiteSpace(district))
        {
            query = query.Where(m => m.District == district);
        }
        return await query.OrderBy(m => m.District).ThenBy(m => m.NameBn).ToListAsync(cancellationToken);
    }

    public async Task<Market?> GetByCodeAsync(string marketCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(m => m.MarketCode == marketCode && !m.IsDeleted, cancellationToken);
    }
}

public class PriceSourceRepository : GenericRepository<PriceSource>, IPriceSourceRepository
{
    public PriceSourceRepository(KrishiBondhuDbContext context) : base(context) { }

    public async Task<IReadOnlyList<PriceSource>> GetActiveSourcesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(ps => ps.IsActive && !ps.IsDeleted)
            .OrderBy(ps => ps.SourceType)
            .ThenBy(ps => ps.NameBn)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PriceSource>> GetOfficialSourcesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(ps => ps.IsActive && !ps.IsDeleted && ps.IsOfficialAuthority)
            .OrderBy(ps => ps.NameBn)
            .ToListAsync(cancellationToken);
    }
}

public class MarketPriceObservationRepository : GenericRepository<MarketPriceObservation>, IMarketPriceObservationRepository
{
    public MarketPriceObservationRepository(KrishiBondhuDbContext context) : base(context) { }

    public async Task<PaginatedList<MarketPriceObservation>> GetObservationsAsync(KrishiBondhu.Application.DTOs.MarketPriceFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking()
            .Include(o => o.Crop)
            .Include(o => o.Market)
            .Include(o => o.PriceSource)
            .Where(o => !o.IsDeleted);

        if (filter.CropId.HasValue)
        {
            query = query.Where(o => o.CropId == filter.CropId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            query = query.Where(o => o.Crop != null && o.Crop.Category == filter.Category);
        }

        if (filter.MarketType.HasValue)
        {
            query = query.Where(o => o.MarketType == filter.MarketType.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.District))
        {
            query = query.Where(o => o.Market != null && o.Market.District == filter.District);
        }

        if (!string.IsNullOrWhiteSpace(filter.Upazila))
        {
            query = query.Where(o => o.Market != null && o.Market.Upazila == filter.Upazila);
        }

        if (filter.MarketId.HasValue)
        {
            query = query.Where(o => o.MarketId == filter.MarketId.Value);
        }

        if (filter.Date.HasValue)
        {
            var date = filter.Date.Value.Date;
            query = query.Where(o => o.ObservationDate.Date == date);
        }

        if (filter.PriceSourceId.HasValue)
        {
            query = query.Where(o => o.PriceSourceId == filter.PriceSourceId.Value);
        }

        if (filter.VerificationStatus.HasValue)
        {
            query = query.Where(o => o.VerificationStatus == filter.VerificationStatus.Value);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.ObservationDate)
            .ThenByDescending(o => o.CreatedAtUtc)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<MarketPriceObservation>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<IReadOnlyList<MarketPriceObservation>> GetObservationsForCropAndDateAsync(Guid cropId, DateTime date, MarketType? marketType = null, string? district = null, CancellationToken cancellationToken = default)
    {
        var targetDate = date.Date;
        var query = _dbSet.AsNoTracking()
            .Include(o => o.Market)
            .Include(o => o.PriceSource)
            .Include(o => o.Crop)
            .Where(o => o.CropId == cropId && o.ObservationDate.Date == targetDate && !o.IsDeleted && o.VerificationStatus == PriceVerificationStatus.Verified);

        if (marketType.HasValue)
        {
            query = query.Where(o => o.MarketType == marketType.Value);
        }

        if (!string.IsNullOrWhiteSpace(district))
        {
            query = query.Where(o => o.Market != null && o.Market.District == district);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MarketPriceObservation>> GetObservationsForCropAndDateRangeAsync(Guid cropId, DateTime fromDate, DateTime toDate, string? district = null, MarketType? marketType = null, CancellationToken cancellationToken = default)
    {
        var start = fromDate.Date;
        var end = toDate.Date;

        var query = _dbSet.AsNoTracking()
            .Include(o => o.Market)
            .Where(o => o.CropId == cropId && o.ObservationDate.Date >= start && o.ObservationDate.Date <= end && !o.IsDeleted && o.VerificationStatus == PriceVerificationStatus.Verified);

        if (marketType.HasValue)
        {
            query = query.Where(o => o.MarketType == marketType.Value);
        }

        if (!string.IsNullOrWhiteSpace(district))
        {
            query = query.Where(o => o.Market != null && o.Market.District == district);
        }

        return await query.OrderBy(o => o.ObservationDate).ToListAsync(cancellationToken);
    }

    public async Task<MarketPriceObservation?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Crop)
            .Include(o => o.Market)
            .Include(o => o.PriceSource)
            .Include(o => o.MarketPriceReport)
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsDuplicateObservationAsync(Guid cropId, Guid marketId, MarketType marketType, DateTime observationDate, Guid priceSourceId, QualityGrade grade, CancellationToken cancellationToken = default)
    {
        var date = observationDate.Date;
        return await _dbSet.AsNoTracking().AnyAsync(o =>
            o.CropId == cropId &&
            o.MarketId == marketId &&
            o.MarketType == marketType &&
            o.ObservationDate.Date == date &&
            o.PriceSourceId == priceSourceId &&
            o.Grade == grade &&
            !o.IsDeleted, cancellationToken);
    }
}

public class OfficialPriceDirectiveRepository : GenericRepository<OfficialPriceDirective>, IOfficialPriceDirectiveRepository
{
    public OfficialPriceDirectiveRepository(KrishiBondhuDbContext context) : base(context) { }

    public async Task<IReadOnlyList<OfficialPriceDirective>> GetActiveDirectivesForCropAsync(Guid cropId, DateTime date, CancellationToken cancellationToken = default)
    {
        var dateUtc = date.Kind == DateTimeKind.Utc ? date : date.ToUniversalTime();

        return await _dbSet.AsNoTracking()
            .Include(d => d.Crop)
            .Where(d => d.CropId == cropId &&
                        d.IsActive &&
                        !d.IsDeleted &&
                        d.VerificationStatus == PriceVerificationStatus.Verified &&
                        d.EffectiveFromUtc <= dateUtc &&
                        (d.EffectiveToUtc == null || d.EffectiveToUtc >= dateUtc))
            .OrderByDescending(d => d.EffectiveFromUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OfficialPriceDirective>> GetDirectivesAsync(Guid? cropId = null, string? district = null, bool? activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().Include(d => d.Crop).Where(d => !d.IsDeleted);

        if (cropId.HasValue)
        {
            query = query.Where(d => d.CropId == cropId.Value);
        }

        if (activeOnly.HasValue && activeOnly.Value)
        {
            var now = DateTime.UtcNow;
            query = query.Where(d => d.IsActive && d.EffectiveFromUtc <= now && (d.EffectiveToUtc == null || d.EffectiveToUtc >= now));
        }

        if (!string.IsNullOrWhiteSpace(district))
        {
            query = query.Where(d => d.ApplicableDistrict == null || d.ApplicableDistrict == district);
        }

        return await query.OrderByDescending(d => d.EffectiveFromUtc).ToListAsync(cancellationToken);
    }

    public async Task<OfficialPriceDirective?> GetByDirectiveNumberAsync(string directiveNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Include(d => d.Crop)
            .FirstOrDefaultAsync(d => d.DirectiveNumber == directiveNumber && !d.IsDeleted, cancellationToken);
    }
}

public class ConsumerComplaintRepository : GenericRepository<ConsumerComplaint>, IConsumerComplaintRepository
{
    public ConsumerComplaintRepository(KrishiBondhuDbContext context) : base(context) { }

    public async Task<ConsumerComplaint?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Crop)
            .Include(c => c.Market)
            .Include(c => c.OfficialPriceDirective)
            .FirstOrDefaultAsync(c => c.TrackingNumber == trackingNumber && !c.IsDeleted, cancellationToken);
    }

    public async Task<ConsumerComplaint?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Crop)
            .Include(c => c.Market)
            .Include(c => c.OfficialPriceDirective)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    public async Task<PaginatedList<ConsumerComplaint>> SearchComplaintsAsync(KrishiBondhu.Application.DTOs.ComplaintFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking()
            .Include(c => c.Crop)
            .Include(c => c.Market)
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(c => c.TrackingNumber.Contains(term) ||
                                     c.ShopOrBusinessName.Contains(term) ||
                                     c.ShopAddress.Contains(term) ||
                                     c.ComplainantPhone.Contains(term));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(c => c.Status == filter.Status.Value);
        }

        if (filter.CropId.HasValue)
        {
            query = query.Where(c => c.CropId == filter.CropId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.District))
        {
            query = query.Where(c => c.District == filter.District);
        }

        if (filter.SellerType.HasValue)
        {
            query = query.Where(c => c.SellerType == filter.SellerType.Value);
        }

        if (filter.FromDate.HasValue)
        {
            var from = filter.FromDate.Value.Date;
            query = query.Where(c => c.PurchaseDateTime.Date >= from);
        }

        if (filter.ToDate.HasValue)
        {
            var to = filter.ToDate.Value.Date;
            query = query.Where(c => c.PurchaseDateTime.Date <= to);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.CreatedAtUtc)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<ConsumerComplaint>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<string> GenerateNextTrackingNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"CMP-{year}-";
        var count = await _dbSet.CountAsync(c => c.TrackingNumber.StartsWith(prefix), cancellationToken);
        return $"{prefix}{(count + 1):D5}";
    }

    public async Task<int> GetRecentSubmissionsCountByIpAsync(string clientIpHash, DateTime sinceUtc, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(clientIpHash)) return 0;
        return await _dbSet.AsNoTracking()
            .CountAsync(c => c.ClientIpHash == clientIpHash && c.CreatedAtUtc >= sinceUtc, cancellationToken);
    }
}
