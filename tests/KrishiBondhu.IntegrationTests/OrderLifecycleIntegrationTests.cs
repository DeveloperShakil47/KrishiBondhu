using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using KrishiBondhu.Application.Common.Interfaces;
using KrishiBondhu.Application.DTOs;
using KrishiBondhu.Application.Interfaces.Services;
using KrishiBondhu.Domain.Entities;
using KrishiBondhu.Domain.Enums;
using KrishiBondhu.Infrastructure.Repositories;
using KrishiBondhu.Infrastructure.Services;

namespace KrishiBondhu.IntegrationTests;

public class OrderLifecycleIntegrationTests : IAsyncLifetime
{
    private TestDatabaseFixture _fixture = null!;

    public Task InitializeAsync()
    {
        _fixture = new TestDatabaseFixture();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    [Fact]
    public async Task AcceptOfferAndCreateOrder_CreatesOrderWithImmutableSnapshot_AndUpdatesLotStatus()
    {
        // Arrange
        var context = _fixture.Context;
        var unitOfWork = new UnitOfWork(context);
        var lotRepo = new HarvestLotRepository(context);
        var offerRepo = new BuyerOfferRepository(context);
        var orderRepo = new OrderRepository(context);
        var farmerRepo = new FarmerProfileRepository(context);
        var buyerRepo = new BuyerProfileRepository(context);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(u => u.UserId).Returns(_fixture.SeedFarmer.UserId);
        currentUserServiceMock.Setup(u => u.Role).Returns(UserRole.Farmer);

        var auditLoggerMock = new Mock<IAuditLogger>();
        var notificationMock = new Mock<INotificationService>();

        var orderService = new OrderLifecycleService(
            orderRepo,
            offerRepo,
            lotRepo,
            farmerRepo,
            buyerRepo,
            unitOfWork,
            currentUserServiceMock.Object,
            auditLoggerMock.Object,
            notificationMock.Object);

        // Create Harvest Lot
        var lot = new HarvestLot
        {
            LotNumber = "LOT-2026-TEST-001",
            FarmerProfileId = _fixture.SeedFarmer.Id,
            CropId = _fixture.SeedCrop.Id,
            Quantity = 50,
            Unit = WeightUnit.Mon, // 2000 Kg
            AskingPricePerKg = 35m,
            PickupDistrict = "বগুড়া",
            PickupUpazila = "শিবগঞ্জ",
            Status = LotStatus.ActiveListed
        };
        await lotRepo.AddAsync(lot);

        // Create Buyer Offer 1 (Target offer)
        var offer1 = new BuyerOffer
        {
            OfferCode = "OFR-2026-TEST-001",
            HarvestLotId = lot.Id,
            BuyerProfileId = _fixture.SeedBuyer.Id,
            OfferedPricePerKg = 36m,
            OfferedQuantityKg = 2000m,
            TransportCostDeduction = 1500m,
            WeighingAndLaborCostDeduction = 600m,
            PackagingCostDeduction = 800m,
            PlatformFee = 360m,
            ExpiryDateUtc = DateTime.UtcNow.AddDays(5),
            Status = OfferStatus.Submitted
        };
        await offerRepo.AddAsync(offer1);

        // Create Buyer Offer 2 (Competing offer)
        var offer2 = new BuyerOffer
        {
            OfferCode = "OFR-2026-TEST-002",
            HarvestLotId = lot.Id,
            BuyerProfileId = _fixture.SeedBuyer.Id,
            OfferedPricePerKg = 34m,
            OfferedQuantityKg = 2000m,
            ExpiryDateUtc = DateTime.UtcNow.AddDays(5),
            Status = OfferStatus.Submitted
        };
        await offerRepo.AddAsync(offer2);
        await unitOfWork.SaveChangesAsync();

        var acceptDto = new AcceptOfferDto
        {
            BuyerOfferId = offer1.Id,
            IdempotencyKey = "TEST-IDEMPOTENCY-KEY-001"
        };

        // Act
        var result = await orderService.AcceptOfferAndCreateOrderAsync(acceptDto);

        // Assert
        result.Succeeded.Should().BeTrue(string.Join("; ", result.Errors));
        result.Data.Should().NotBeNull();
        result.Data!.OrderNumber.Should().StartWith("KB-");

        // Verify Database Persistence
        var persistedOrder = await orderRepo.GetWithFullDetailsAsync(result.Data.Id);
        persistedOrder.Should().NotBeNull();
        persistedOrder!.AgreedPricePerKg.Should().Be(36m);
        persistedOrder.AgreedQuantityKg.Should().Be(2000m);
        persistedOrder.Status.Should().Be(OrderStatus.OfferAccepted);
        persistedOrder.TermsSnapshotJson.Should().Contain("OFR-2026-TEST-001");

        // Verify Lot status changed to Sold
        var updatedLot = await lotRepo.GetByIdAsync(lot.Id);
        updatedLot!.Status.Should().Be(LotStatus.Sold);

        // Verify Accepted offer is Marked Accepted
        var updatedOffer1 = await offerRepo.GetByIdAsync(offer1.Id);
        updatedOffer1!.Status.Should().Be(OfferStatus.Accepted);

        // Verify Competing offer was Rejected
        var updatedOffer2 = await offerRepo.GetByIdAsync(offer2.Id);
        updatedOffer2!.Status.Should().Be(OfferStatus.Rejected);
    }

    [Fact]
    public async Task AcceptOffer_WhenCalledTwiceWithSameIdempotencyKey_ReturnsSameOrderWithoutDuplicate()
    {
        // Arrange
        var context = _fixture.Context;
        var unitOfWork = new UnitOfWork(context);
        var lotRepo = new HarvestLotRepository(context);
        var offerRepo = new BuyerOfferRepository(context);
        var orderRepo = new OrderRepository(context);
        var farmerRepo = new FarmerProfileRepository(context);
        var buyerRepo = new BuyerProfileRepository(context);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(u => u.UserId).Returns(_fixture.SeedFarmer.UserId);
        currentUserServiceMock.Setup(u => u.Role).Returns(UserRole.Farmer);

        var auditLoggerMock = new Mock<IAuditLogger>();
        var notificationMock = new Mock<INotificationService>();

        var orderService = new OrderLifecycleService(
            orderRepo,
            offerRepo,
            lotRepo,
            farmerRepo,
            buyerRepo,
            unitOfWork,
            currentUserServiceMock.Object,
            auditLoggerMock.Object,
            notificationMock.Object);

        var lot = new HarvestLot
        {
            LotNumber = "LOT-2026-IDEM-001",
            FarmerProfileId = _fixture.SeedFarmer.Id,
            CropId = _fixture.SeedCrop.Id,
            Quantity = 20,
            Unit = WeightUnit.Mon,
            AskingPricePerKg = 30m,
            PickupDistrict = "বগুড়া",
            PickupUpazila = "শিবগঞ্জ",
            Status = LotStatus.ActiveListed
        };
        await lotRepo.AddAsync(lot);

        var offer = new BuyerOffer
        {
            OfferCode = "OFR-2026-IDEM-001",
            HarvestLotId = lot.Id,
            BuyerProfileId = _fixture.SeedBuyer.Id,
            OfferedPricePerKg = 30m,
            OfferedQuantityKg = 800m,
            ExpiryDateUtc = DateTime.UtcNow.AddDays(3),
            Status = OfferStatus.Submitted
        };
        await offerRepo.AddAsync(offer);
        await unitOfWork.SaveChangesAsync();

        var idempotencyKey = "UNIQUE-SUBMISSION-KEY-999";
        var acceptDto = new AcceptOfferDto
        {
            BuyerOfferId = offer.Id,
            IdempotencyKey = idempotencyKey
        };

        // Act 1: First submission
        var result1 = await orderService.AcceptOfferAndCreateOrderAsync(acceptDto);

        // Act 2: Repeated submission (double-click)
        var result2 = await orderService.AcceptOfferAndCreateOrderAsync(acceptDto);

        // Assert
        result1.Succeeded.Should().BeTrue();
        result2.Succeeded.Should().BeTrue();
        result1.Data!.Id.Should().Be(result2.Data!.Id);
        result1.Data.OrderNumber.Should().Be(result2.Data.OrderNumber);

        var allOrders = await orderRepo.GetAllAsync();
        allOrders.Count(o => o.IdempotencyKey == idempotencyKey).Should().Be(1);
    }
}
