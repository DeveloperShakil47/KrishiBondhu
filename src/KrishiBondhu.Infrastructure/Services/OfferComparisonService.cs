using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using KrishiBondhu.Application.Common.Interfaces;
using KrishiBondhu.Application.Common.Models;
using KrishiBondhu.Application.DTOs;
using KrishiBondhu.Application.Interfaces.Repositories;
using KrishiBondhu.Application.Interfaces.Services;
using KrishiBondhu.Domain.Entities;
using KrishiBondhu.Domain.Enums;
using KrishiBondhu.Infrastructure.Data;

namespace KrishiBondhu.Infrastructure.Services;

public class OfferComparisonService : IOfferComparisonService
{
    private readonly IBuyerOfferRepository _offerRepository;
    private readonly IHarvestLotRepository _lotRepository;
    private readonly IBuyerProfileRepository _buyerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogger _auditLogger;
    private readonly decimal _platformFeePercentage;

    public OfferComparisonService(
        IBuyerOfferRepository offerRepository,
        IHarvestLotRepository lotRepository,
        IBuyerProfileRepository buyerRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditLogger auditLogger,
        IConfiguration configuration)
    {
        _offerRepository = offerRepository;
        _lotRepository = lotRepository;
        _buyerRepository = buyerRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _auditLogger = auditLogger;

        var feeSetting = configuration["PlatformSettings:PlatformFeePercentage"];
        _platformFeePercentage = decimal.TryParse(feeSetting, out var fee) ? fee : 0.5m;
    }

    public async Task<Result<BuyerOfferDto>> SubmitOfferAsync(CreateBuyerOfferDto dto, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (!currentUserId.HasValue)
        {
            return Result<BuyerOfferDto>.Failure("অননুমোদিত ব্যবহারকারী। লগইন করুন।");
        }

        var buyer = await _buyerRepository.GetByUserIdAsync(currentUserId.Value, cancellationToken);
        if (buyer == null)
        {
            return Result<BuyerOfferDto>.Failure("শুধুমাত্র অনুমোদিত ব্যবসায়ীরা দরপ্রস্তাব দিতে পারবেন।");
        }

        var lot = await _lotRepository.GetWithDetailsAsync(dto.HarvestLotId, cancellationToken);
        if (lot == null || lot.Status == LotStatus.Sold || lot.Status == LotStatus.Cancelled)
        {
            return Result<BuyerOfferDto>.Failure("ফসলের লটটি বর্তমানে সক্রিয় বা উপলব্ধ নেই।");
        }

        if (dto.OfferedPricePerKg <= 0 || dto.OfferedQuantityKg <= 0)
        {
            return Result<BuyerOfferDto>.Failure("দর এবং পরিমাণ অবশ্যই শূন্যের বেশি হতে হবে।");
        }

        if (dto.ExpiryDateUtc <= DateTime.UtcNow)
        {
            return Result<BuyerOfferDto>.Failure("প্রস্তাবের মেয়াদ ভবিষ্যতের তারিখ হতে হবে।");
        }

        var offerCode = await _offerRepository.GenerateNextOfferCodeAsync(cancellationToken);

        // Calculate gross and platform fee
        var grossValue = Math.Round(dto.OfferedQuantityKg * dto.OfferedPricePerKg, 2, MidpointRounding.AwayFromZero);
        var calculatedPlatformFee = Math.Round(grossValue * (_platformFeePercentage / 100m), 2, MidpointRounding.AwayFromZero);

        var offer = new BuyerOffer
        {
            OfferCode = offerCode,
            HarvestLotId = lot.Id,
            BuyerProfileId = buyer.Id,
            OfferedPricePerKg = dto.OfferedPricePerKg,
            OfferedQuantityKg = dto.OfferedQuantityKg,
            TransportCostDeduction = dto.TransportCostDeduction,
            WeighingAndLaborCostDeduction = dto.WeighingAndLaborCostDeduction,
            PackagingCostDeduction = dto.PackagingCostDeduction,
            PlatformFee = calculatedPlatformFee,
            OtherDeductions = dto.OtherDeductions,
            ExpiryDateUtc = dto.ExpiryDateUtc,
            DeliveryTerms = dto.DeliveryTerms,
            PaymentTerms = dto.PaymentTerms,
            Remarks = dto.Remarks,
            Status = OfferStatus.Submitted
        };

        await _offerRepository.AddAsync(offer, cancellationToken);

        // Update lot status to UnderOffer if currently ActiveListed
        if (lot.Status == LotStatus.ActiveListed)
        {
            lot.Status = LotStatus.UnderOffer;
            await _lotRepository.UpdateAsync(lot, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync("BuyerOffer", offer.Id.ToString(), "SubmitOffer", $"Submitted offer {offerCode} for lot {lot.LotNumber}", cancellationToken);

        return Result<BuyerOfferDto>.Success(MapToDto(offer, lot.LotNumber, lot.Crop?.NameBn ?? "", buyer.BusinessName, buyer.PhoneNumber, buyer.Rating), "দরপ্রস্তাব সফলভাবে পাঠানো হয়েছে।");
    }

    public async Task<OfferComparisonSummaryDto> GetOfferComparisonForLotAsync(Guid harvestLotId, decimal? estimatedProductionCostPerKg = null, CancellationToken cancellationToken = default)
    {
        var lot = await _lotRepository.GetWithDetailsAsync(harvestLotId, cancellationToken);
        if (lot == null)
        {
            return new OfferComparisonSummaryDto();
        }

        var offers = await _offerRepository.GetOffersForLotAsync(harvestLotId, cancellationToken);

        var offerDtos = offers
            .Where(o => o.Status != OfferStatus.Expired && o.Status != OfferStatus.Rejected)
            .Select(o => MapToDto(
                o,
                lot.LotNumber,
                lot.Crop?.NameBn ?? "",
                o.BuyerProfile?.BusinessName ?? "",
                o.BuyerProfile?.PhoneNumber ?? "",
                o.BuyerProfile?.Rating ?? 5.0m))
            .OrderByDescending(o => o.EstimatedNetProceeds)
            .ToList();

        var bestOffer = offerDtos.FirstOrDefault();

        return new OfferComparisonSummaryDto
        {
            HarvestLotId = lot.Id,
            LotNumber = lot.LotNumber,
            CropNameBn = lot.Crop?.NameBn ?? "",
            LotQuantityKg = lot.QuantityInKg,
            AskingPricePerKg = lot.AskingPricePerKg,
            FarmerEstimatedProductionCostPerKg = estimatedProductionCostPerKg ?? 0,
            Offers = offerDtos,
            RecommendedBestOffer = bestOffer
        };
    }

    public async Task<IReadOnlyList<BuyerOfferDto>> GetOffersByBuyerAsync(Guid buyerProfileId, CancellationToken cancellationToken = default)
    {
        var offers = await _offerRepository.GetOffersByBuyerAsync(buyerProfileId, cancellationToken);
        return offers.Select(o => MapToDto(
            o,
            o.HarvestLot?.LotNumber ?? "",
            o.HarvestLot?.Crop?.NameBn ?? "",
            o.BuyerProfile?.BusinessName ?? "",
            o.BuyerProfile?.PhoneNumber ?? "",
            o.BuyerProfile?.Rating ?? 5.0m)).ToList();
    }

    public async Task<BuyerOfferDto?> GetOfferByIdAsync(Guid offerId, CancellationToken cancellationToken = default)
    {
        var o = await _offerRepository.GetWithDetailsAsync(offerId, cancellationToken);
        if (o == null) return null;

        return MapToDto(
            o,
            o.HarvestLot?.LotNumber ?? "",
            o.HarvestLot?.Crop?.NameBn ?? "",
            o.BuyerProfile?.BusinessName ?? "",
            o.BuyerProfile?.PhoneNumber ?? "",
            o.BuyerProfile?.Rating ?? 5.0m);
    }

    private static BuyerOfferDto MapToDto(
        BuyerOffer o,
        string lotNumber,
        string cropName,
        string buyerBusiness,
        string buyerPhone,
        decimal buyerRating)
    {
        return new BuyerOfferDto
        {
            Id = o.Id,
            OfferCode = o.OfferCode,
            HarvestLotId = o.HarvestLotId,
            LotNumber = lotNumber,
            CropNameBn = cropName,
            BuyerProfileId = o.BuyerProfileId,
            BuyerBusinessName = buyerBusiness,
            BuyerContactPhone = buyerPhone,
            BuyerRating = buyerRating,
            OfferedPricePerKg = o.OfferedPricePerKg,
            OfferedQuantityKg = o.OfferedQuantityKg,
            TransportCostDeduction = o.TransportCostDeduction,
            WeighingAndLaborCostDeduction = o.WeighingAndLaborCostDeduction,
            PackagingCostDeduction = o.PackagingCostDeduction,
            PlatformFee = o.PlatformFee,
            OtherDeductions = o.OtherDeductions,
            GrossSaleValue = o.GrossSaleValue,
            TotalEstimatedDeductions = o.TotalEstimatedDeductions,
            EstimatedNetProceeds = o.EstimatedNetProceeds,
            ExpiryDateUtc = o.ExpiryDateUtc,
            DeliveryTerms = o.DeliveryTerms,
            PaymentTerms = o.PaymentTerms,
            Remarks = o.Remarks,
            Status = o.Status,
            CreatedAtUtc = o.CreatedAtUtc
        };
    }
}
