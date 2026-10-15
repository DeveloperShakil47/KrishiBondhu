using System;
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
using KrishiBondhu.Domain.Exceptions;
using KrishiBondhu.Infrastructure.Data;

namespace KrishiBondhu.Infrastructure.Services;

public class FarmerService : IFarmerService
{
    private readonly IFarmerProfileRepository _farmerRepository;
    private readonly IRepository<FarmPlot> _plotRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogger _auditLogger;

    public FarmerService(
        IFarmerProfileRepository farmerRepository,
        IRepository<FarmPlot> plotRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditLogger auditLogger)
    {
        _farmerRepository = farmerRepository;
        _plotRepository = plotRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _auditLogger = auditLogger;
    }

    public async Task<FarmerProfileDto?> GetProfileByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await _farmerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (profile == null) return null;

        return MapToDto(profile);
    }

    public async Task<Result<FarmerProfileDto>> CreateOrUpdateProfileAsync(FarmerProfileDto dto, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (!currentUserId.HasValue)
        {
            return Result<FarmerProfileDto>.Failure("অননুমোদিত ব্যবহারকারী।");
        }

        var profile = await _farmerRepository.GetByUserIdAsync(currentUserId.Value, cancellationToken);
        if (profile == null)
        {
            profile = new FarmerProfile
            {
                UserId = currentUserId.Value,
                FullNameBn = dto.FullNameBn,
                FullNameEn = dto.FullNameEn,
                PhoneNumber = dto.PhoneNumber,
                NationalIdNumber = dto.NationalIdNumber,
                District = dto.District,
                Upazila = dto.Upazila,
                Union = dto.Union,
                VillageOrRoad = dto.VillageOrRoad,
                TotalLandAreaAcres = dto.TotalLandAreaAcres,
                FarmingExperienceYears = dto.FarmingExperienceYears,
                BankAccountNumber = dto.BankAccountNumber,
                BankName = dto.BankName,
                BkashNumber = dto.BkashNumber,
                NagadNumber = dto.NagadNumber,
                IsActive = true
            };
            await _farmerRepository.AddAsync(profile, cancellationToken);
        }
        else
        {
            profile.FullNameBn = dto.FullNameBn;
            profile.FullNameEn = dto.FullNameEn;
            profile.NationalIdNumber = dto.NationalIdNumber;
            profile.District = dto.District;
            profile.Upazila = dto.Upazila;
            profile.Union = dto.Union;
            profile.VillageOrRoad = dto.VillageOrRoad;
            profile.TotalLandAreaAcres = dto.TotalLandAreaAcres;
            profile.FarmingExperienceYears = dto.FarmingExperienceYears;
            profile.BankAccountNumber = dto.BankAccountNumber;
            profile.BankName = dto.BankName;
            profile.BkashNumber = dto.BkashNumber;
            profile.NagadNumber = dto.NagadNumber;
            await _farmerRepository.UpdateAsync(profile, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditLogger.LogAsync("FarmerProfile", profile.Id.ToString(), "UpdateProfile", cancellationToken: cancellationToken);

        return Result<FarmerProfileDto>.Success(MapToDto(profile), "কৃষকের প্রোফাইল সফলভাবে হালনাগাদ করা হয়েছে।");
    }

    public async Task<Result<FarmPlotDto>> AddPlotAsync(FarmPlotDto plotDto, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        var profile = await _farmerRepository.GetByUserIdAsync(currentUserId ?? Guid.Empty, cancellationToken);
        if (profile == null)
        {
            return Result<FarmPlotDto>.Failure("কৃষক প্রোফাইল পাওয়া যায়নি।");
        }

        var plot = new FarmPlot
        {
            FarmerProfileId = profile.Id,
            PlotName = plotDto.PlotName,
            AreaAcres = plotDto.AreaAcres,
            SoilType = plotDto.SoilType,
            IrrigationSource = plotDto.IrrigationSource,
            CurrentCropName = plotDto.CurrentCropName
        };

        await _plotRepository.AddAsync(plot, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        plotDto.Id = plot.Id;
        plotDto.FarmerProfileId = profile.Id;
        return Result<FarmPlotDto>.Success(plotDto, "নতুন জমির তথ্য সংরক্ষিত হয়েছে।");
    }

    public async Task<Result> DeletePlotAsync(Guid plotId, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        var profile = await _farmerRepository.GetByUserIdAsync(currentUserId ?? Guid.Empty, cancellationToken);
        var plot = await _plotRepository.GetByIdAsync(plotId, cancellationToken);

        if (plot == null || profile == null || plot.FarmerProfileId != profile.Id)
        {
            return Result.Failure("জমির রেকর্ড পাওয়া যায়নি বা অধিকার নেই।");
        }

        await _plotRepository.DeleteAsync(plot, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("জমির তথ্য মুছে ফেলা হয়েছে।");
    }

    private static FarmerProfileDto MapToDto(FarmerProfile profile)
    {
        return new FarmerProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FullNameBn = profile.FullNameBn,
            FullNameEn = profile.FullNameEn,
            PhoneNumber = profile.PhoneNumber,
            NationalIdNumber = profile.NationalIdNumber,
            IsNidVerified = profile.IsNidVerified,
            District = profile.District,
            Upazila = profile.Upazila,
            Union = profile.Union,
            VillageOrRoad = profile.VillageOrRoad,
            TotalLandAreaAcres = profile.TotalLandAreaAcres,
            FarmingExperienceYears = profile.FarmingExperienceYears,
            BankAccountNumber = profile.BankAccountNumber,
            BankName = profile.BankName,
            BkashNumber = profile.BkashNumber,
            NagadNumber = profile.NagadNumber,
            Plots = profile.Plots.Where(p => !p.IsDeleted).Select(p => new FarmPlotDto
            {
                Id = p.Id,
                FarmerProfileId = p.FarmerProfileId,
                PlotName = p.PlotName,
                AreaAcres = p.AreaAcres,
                SoilType = p.SoilType,
                IrrigationSource = p.IrrigationSource,
                CurrentCropName = p.CurrentCropName
            }).ToList()
        };
    }
}

public class BuyerService : IBuyerService
{
    private readonly IBuyerProfileRepository _buyerRepository;
    private readonly IRepository<BuyerDemand> _demandRepository;
    private readonly KrishiBondhuDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogger _auditLogger;

    public BuyerService(
        IBuyerProfileRepository buyerRepository,
        IRepository<BuyerDemand> demandRepository,
        KrishiBondhuDbContext context,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditLogger auditLogger)
    {
        _buyerRepository = buyerRepository;
        _demandRepository = demandRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _auditLogger = auditLogger;
    }

    public async Task<BuyerProfileDto?> GetProfileByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await _buyerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (profile == null) return null;
        return MapToDto(profile);
    }

    public async Task<Result<BuyerProfileDto>> CreateOrUpdateProfileAsync(BuyerProfileDto dto, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (!currentUserId.HasValue)
        {
            return Result<BuyerProfileDto>.Failure("অননুমোদিত ব্যবহারকারী।");
        }

        var profile = await _buyerRepository.GetByUserIdAsync(currentUserId.Value, cancellationToken);
        if (profile == null)
        {
            profile = new BuyerProfile
            {
                UserId = currentUserId.Value,
                BusinessName = dto.BusinessName,
                ContactPersonName = dto.ContactPersonName,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                BusinessType = dto.BusinessType,
                TradeLicenseNumber = dto.TradeLicenseNumber,
                District = dto.District,
                Upazila = dto.Upazila,
                WarehouseAddress = dto.WarehouseAddress,
                IsActive = true
            };
            await _buyerRepository.AddAsync(profile, cancellationToken);
        }
        else
        {
            profile.BusinessName = dto.BusinessName;
            profile.ContactPersonName = dto.ContactPersonName;
            profile.BusinessType = dto.BusinessType;
            profile.TradeLicenseNumber = dto.TradeLicenseNumber;
            profile.District = dto.District;
            profile.Upazila = dto.Upazila;
            profile.WarehouseAddress = dto.WarehouseAddress;
            await _buyerRepository.UpdateAsync(profile, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditLogger.LogAsync("BuyerProfile", profile.Id.ToString(), "UpdateProfile", cancellationToken: cancellationToken);

        return Result<BuyerProfileDto>.Success(MapToDto(profile), "ব্যবসায়ী প্রোফাইল সফলভাবে সংরক্ষিত হয়েছে।");
    }

    public async Task<PaginatedList<BuyerDemandDto>> GetDemandsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.BuyerDemands
            .AsNoTracking()
            .Include(d => d.BuyerProfile)
            .Include(d => d.Crop)
            .Where(d => !d.IsDeleted && d.IsActive);

        int totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(d => d.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new BuyerDemandDto
            {
                Id = d.Id,
                BuyerProfileId = d.BuyerProfileId,
                BuyerBusinessName = d.BuyerProfile != null ? d.BuyerProfile.BusinessName : string.Empty,
                CropId = d.CropId,
                CropNameBn = d.Crop != null ? d.Crop.NameBn : string.Empty,
                DemandTitle = d.DemandTitle,
                RequiredQuantityKg = d.RequiredQuantityKg,
                TargetPricePerKg = d.TargetPricePerKg,
                ExpectedGrade = d.ExpectedGrade,
                TargetDeliveryDate = d.TargetDeliveryDate,
                DeliveryDistrict = d.DeliveryDistrict,
                DeliveryUpazila = d.DeliveryUpazila,
                QualitySpecifications = d.QualitySpecifications,
                IsFulfilled = d.IsFulfilled
            })
            .ToListAsync(cancellationToken);

        return new PaginatedList<BuyerDemandDto>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Result<BuyerDemandDto>> CreateDemandAsync(CreateBuyerDemandDto dto, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        var profile = await _buyerRepository.GetByUserIdAsync(currentUserId ?? Guid.Empty, cancellationToken);
        if (profile == null)
        {
            return Result<BuyerDemandDto>.Failure("শুধুমাত্র অনুমোদিত ব্যবসায়ীরা চাহিদা প্রকাশ করতে পারবেন।");
        }

        var demand = new BuyerDemand
        {
            BuyerProfileId = profile.Id,
            CropId = dto.CropId,
            DemandTitle = dto.DemandTitle,
            RequiredQuantityKg = dto.RequiredQuantityKg,
            TargetPricePerKg = dto.TargetPricePerKg,
            ExpectedGrade = dto.ExpectedGrade,
            TargetDeliveryDate = dto.TargetDeliveryDate,
            DeliveryDistrict = dto.DeliveryDistrict,
            DeliveryUpazila = dto.DeliveryUpazila,
            DeliveryAddress = $"{dto.DeliveryUpazila}, {dto.DeliveryDistrict}",
            QualitySpecifications = dto.QualitySpecifications,
            IsFulfilled = false,
            IsActive = true
        };

        await _demandRepository.AddAsync(demand, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var crop = await _context.Crops.FindAsync(new object[] { dto.CropId }, cancellationToken);

        var resultDto = new BuyerDemandDto
        {
            Id = demand.Id,
            BuyerProfileId = profile.Id,
            BuyerBusinessName = profile.BusinessName,
            CropId = demand.CropId,
            CropNameBn = crop?.NameBn ?? string.Empty,
            DemandTitle = demand.DemandTitle,
            RequiredQuantityKg = demand.RequiredQuantityKg,
            TargetPricePerKg = demand.TargetPricePerKg,
            ExpectedGrade = demand.ExpectedGrade,
            TargetDeliveryDate = demand.TargetDeliveryDate,
            DeliveryDistrict = demand.DeliveryDistrict,
            DeliveryUpazila = demand.DeliveryUpazila,
            QualitySpecifications = demand.QualitySpecifications,
            IsFulfilled = false
        };

        return Result<BuyerDemandDto>.Success(resultDto, "ফসলের চাহিদা সফলভাবে পোস্ট করা হয়েছে।");
    }

    private static BuyerProfileDto MapToDto(BuyerProfile profile)
    {
        return new BuyerProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            BusinessName = profile.BusinessName,
            ContactPersonName = profile.ContactPersonName,
            PhoneNumber = profile.PhoneNumber,
            Email = profile.Email,
            BusinessType = profile.BusinessType,
            TradeLicenseNumber = profile.TradeLicenseNumber,
            IsTradeLicenseVerified = profile.IsTradeLicenseVerified,
            District = profile.District,
            Upazila = profile.Upazila,
            WarehouseAddress = profile.WarehouseAddress,
            Rating = profile.Rating,
            CompletedTransactionsCount = profile.CompletedTransactionsCount
        };
    }
}
