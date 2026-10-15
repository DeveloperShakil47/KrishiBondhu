using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KrishiBondhu.Application.Common.Interfaces;
using KrishiBondhu.Application.Common.Models;
using KrishiBondhu.Application.DTOs;
using KrishiBondhu.Application.Interfaces.Repositories;
using KrishiBondhu.Application.Interfaces.Services;
using KrishiBondhu.Domain.Entities;
using KrishiBondhu.Domain.Enums;
using KrishiBondhu.Infrastructure.Data;

namespace KrishiBondhu.Infrastructure.Services;

public class CropService : ICropService
{
    private readonly ICropRepository _cropRepository;

    public CropService(ICropRepository cropRepository)
    {
        _cropRepository = cropRepository;
    }

    public async Task<IReadOnlyList<CropDto>> GetAllCropsAsync(CancellationToken cancellationToken = default)
    {
        var crops = await _cropRepository.GetActiveCropsAsync(cancellationToken);
        return crops.Select(c => new CropDto
        {
            Id = c.Id,
            NameBn = c.NameBn,
            NameEn = c.NameEn,
            Category = c.Category,
            DescriptionBn = c.DescriptionBn,
            StandardUnit = c.StandardUnit,
            TypicalMinPricePerKg = c.TypicalMinPricePerKg,
            TypicalMaxPricePerKg = c.TypicalMaxPricePerKg,
            IconEmoji = c.IconEmoji
        }).ToList();
    }

    public async Task<CropDto?> GetCropByIdAsync(Guid cropId, CancellationToken cancellationToken = default)
    {
        var c = await _cropRepository.GetByIdAsync(cropId, cancellationToken);
        if (c == null) return null;

        return new CropDto
        {
            Id = c.Id,
            NameBn = c.NameBn,
            NameEn = c.NameEn,
            Category = c.Category,
            DescriptionBn = c.DescriptionBn,
            StandardUnit = c.StandardUnit,
            TypicalMinPricePerKg = c.TypicalMinPricePerKg,
            TypicalMaxPricePerKg = c.TypicalMaxPricePerKg,
            IconEmoji = c.IconEmoji
        };
    }
}

public class HarvestLotService : IHarvestLotService
{
    private readonly IHarvestLotRepository _lotRepository;
    private readonly IFarmerProfileRepository _farmerRepository;
    private readonly ICropRepository _cropRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogger _auditLogger;

    public HarvestLotService(
        IHarvestLotRepository lotRepository,
        IFarmerProfileRepository farmerRepository,
        ICropRepository cropRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditLogger auditLogger)
    {
        _lotRepository = lotRepository;
        _farmerRepository = farmerRepository;
        _cropRepository = cropRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _auditLogger = auditLogger;
    }

    public async Task<Result<HarvestLotDto>> CreateLotAsync(CreateHarvestLotDto dto, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result<HarvestLotDto>.Failure("অননুমোদিত ব্যবহারকারী। লগইন করুন।");
        }

        var farmer = await _farmerRepository.GetByUserIdAsync(userId.Value, cancellationToken);
        if (farmer == null)
        {
            return Result<HarvestLotDto>.Failure("লট তৈরির জন্য কৃষক প্রোফাইল থাকা আবশ্যক।");
        }

        var crop = await _cropRepository.GetByIdAsync(dto.CropId, cancellationToken);
        if (crop == null)
        {
            return Result<HarvestLotDto>.Failure("নির্বাচিত ফসলটি ডাটাবেজে পাওয়া যায়নি।");
        }

        if (dto.Quantity <= 0)
        {
            return Result<HarvestLotDto>.Failure("ফসলের পরিমাণ অবশ্যই শূন্যের চেয়ে বেশি হতে হবে।");
        }

        if (dto.AskingPricePerKg <= 0)
        {
            return Result<HarvestLotDto>.Failure("প্রতি কেজির দাম শূন্যের চেয়ে বেশি হতে হবে।");
        }

        var districtCode = dto.PickupDistrict.Length >= 3 ? dto.PickupDistrict[..3] : "BD";
        var lotNumber = await _lotRepository.GenerateNextLotNumberAsync(districtCode, cancellationToken);

        var lot = new HarvestLot
        {
            LotNumber = lotNumber,
            FarmerProfileId = farmer.Id,
            CropId = crop.Id,
            Quantity = dto.Quantity,
            Unit = dto.Unit,
            Grade = dto.Grade,
            GradeDescription = dto.GradeDescription,
            AskingPricePerKg = dto.AskingPricePerKg,
            HarvestDate = dto.HarvestDate,
            AvailableFromDate = dto.AvailableFromDate,
            PickupDistrict = dto.PickupDistrict,
            PickupUpazila = dto.PickupUpazila,
            PickupLocationDetails = dto.PickupLocationDetails,
            Status = LotStatus.ActiveListed,
            Notes = dto.Notes,
            PrimaryPhotoUrl = dto.PrimaryPhotoUrl
        };

        await _lotRepository.AddAsync(lot, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync("HarvestLot", lot.Id.ToString(), "CreateLot", $"Created {lotNumber} for {crop.NameBn}", cancellationToken);

        return Result<HarvestLotDto>.Success(MapToDto(lot, crop.NameBn, crop.Category, farmer.FullNameBn, farmer.PhoneNumber), "ফসলের লট সফলভাবে তালিকাভুক্ত হয়েছে।");
    }

    public async Task<HarvestLotDto?> GetLotByIdAsync(Guid lotId, CancellationToken cancellationToken = default)
    {
        var lot = await _lotRepository.GetWithDetailsAsync(lotId, cancellationToken);
        if (lot == null) return null;

        return MapToDto(lot, lot.Crop?.NameBn ?? "", lot.Crop?.Category ?? "", lot.FarmerProfile?.FullNameBn ?? "", lot.FarmerProfile?.PhoneNumber ?? "");
    }

    public async Task<PaginatedList<HarvestLotDto>> SearchLotsAsync(
        string? searchTerm,
        Guid? cropId,
        string? district,
        LotStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var paginated = await _lotRepository.SearchLotsAsync(searchTerm, cropId, district, status, pageNumber, pageSize, cancellationToken);
        var dtos = paginated.Items.Select(h => MapToDto(
            h,
            h.Crop?.NameBn ?? "",
            h.Crop?.Category ?? "",
            h.FarmerProfile?.FullNameBn ?? "",
            h.FarmerProfile?.PhoneNumber ?? "")).ToList();

        return new PaginatedList<HarvestLotDto>(dtos, paginated.TotalCount, paginated.PageNumber, pageSize);
    }

    public async Task<IReadOnlyList<HarvestLotDto>> GetMyLotsAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue) return Array.Empty<HarvestLotDto>();

        var farmer = await _farmerRepository.GetByUserIdAsync(userId.Value, cancellationToken);
        if (farmer == null) return Array.Empty<HarvestLotDto>();

        var lots = await _lotRepository.GetByFarmerIdAsync(farmer.Id, cancellationToken);
        return lots.Select(h => MapToDto(
            h,
            h.Crop?.NameBn ?? "",
            h.Crop?.Category ?? "",
            farmer.FullNameBn,
            farmer.PhoneNumber)).ToList();
    }

    public async Task<Result> UpdateLotStatusAsync(Guid lotId, LotStatus status, CancellationToken cancellationToken = default)
    {
        var lot = await _lotRepository.GetByIdAsync(lotId, cancellationToken);
        if (lot == null) return Result.Failure("লট পাওয়া যায়নি।");

        lot.Status = status;
        await _lotRepository.UpdateAsync(lot, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync("HarvestLot", lot.Id.ToString(), "UpdateStatus", $"Status changed to {status}", cancellationToken);
        return Result.Success("লটের অবস্থা হালনাগাদ করা হয়েছে।");
    }

    private static HarvestLotDto MapToDto(HarvestLot lot, string cropNameBn, string category, string farmerName, string farmerPhone)
    {
        return new HarvestLotDto
        {
            Id = lot.Id,
            LotNumber = lot.LotNumber,
            FarmerProfileId = lot.FarmerProfileId,
            FarmerNameBn = farmerName,
            FarmerPhone = farmerPhone,
            CropId = lot.CropId,
            CropNameBn = cropNameBn,
            CropCategory = category,
            Quantity = lot.Quantity,
            Unit = lot.Unit,
            QuantityInKg = lot.QuantityInKg,
            Grade = lot.Grade,
            GradeDescription = lot.GradeDescription,
            AskingPricePerKg = lot.AskingPricePerKg,
            TotalExpectedGrossValue = lot.TotalExpectedGrossValue,
            HarvestDate = lot.HarvestDate,
            AvailableFromDate = lot.AvailableFromDate,
            PickupDistrict = lot.PickupDistrict,
            PickupUpazila = lot.PickupUpazila,
            PickupLocationDetails = lot.PickupLocationDetails,
            Status = lot.Status,
            Notes = lot.Notes,
            PrimaryPhotoUrl = lot.PrimaryPhotoUrl,
            OffersCount = lot.Offers?.Count ?? 0,
            CreatedAtUtc = lot.CreatedAtUtc
        };
    }
}
