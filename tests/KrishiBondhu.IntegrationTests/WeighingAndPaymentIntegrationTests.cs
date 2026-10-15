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

public class WeighingAndPaymentIntegrationTests : IAsyncLifetime
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
    public async Task WeighingWorkflow_WhenConfirmedByBothParties_UpdatesOrderVerifiedNetQuantity()
    {
        // Arrange
        var context = _fixture.Context;
        var unitOfWork = new UnitOfWork(context);
        var orderRepo = new OrderRepository(context);
        var weighingRepo = new WeighingRecordRepository(context);
        var auditLoggerMock = new Mock<IAuditLogger>();
        var notificationMock = new Mock<INotificationService>();

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(u => u.UserId).Returns(_fixture.SeedFarmer.UserId);
        currentUserServiceMock.Setup(u => u.Role).Returns(UserRole.Farmer);
        currentUserServiceMock.Setup(u => u.FullName).Returns("মো. আবুল হোসেন");

        var weighingService = new WeighingService(
            weighingRepo,
            orderRepo,
            unitOfWork,
            currentUserServiceMock.Object,
            auditLoggerMock.Object,
            notificationMock.Object);

        // Seed active order
        var order = new Order
        {
            OrderNumber = "KB-2026-TEST-W01",
            IdempotencyKey = Guid.NewGuid().ToString(),
            HarvestLotId = _fixture.SeedLot.Id,
            BuyerOfferId = _fixture.SeedOffer.Id,
            FarmerProfileId = _fixture.SeedFarmer.Id,
            BuyerProfileId = _fixture.SeedBuyer.Id,
            AgreedPricePerKg = 35.00m,
            AgreedQuantityKg = 2000.00m,
            Status = OrderStatus.AwaitingDelivery
        };
        await orderRepo.AddAsync(order);
        await unitOfWork.SaveChangesAsync();

        // Act 1: Record weighing: Gross 2050, Tare 50 -> Net 2000
        var recordDto = new RecordWeighingDto
        {
            OrderId = order.Id,
            GrossWeight = 2050m,
            TareWeight = 50m,
            Unit = WeightUnit.Kg,
            CollectionPointName = "মহাস্থান হাট",
            District = "বগুড়া",
            Upazila = "শিবগঞ্জ",
            ScaleIdentifier = "DIGI-SC-202"
        };
        var recordResult = await weighingService.RecordWeighingAsync(recordDto);

        // Assert 1: Record created, order transitioned to DeliveredPendingConfirmation
        recordResult.Succeeded.Should().BeTrue();
        recordResult.Data.Should().NotBeNull();
        recordResult.Data!.NetWeight.Should().Be(2000m);
        recordResult.Data.IsFarmerConfirmed.Should().BeTrue(); // Confirmed by recording farmer

        var updatedOrder = await orderRepo.GetByIdAsync(order.Id);
        updatedOrder!.Status.Should().Be(OrderStatus.DeliveredPendingConfirmation);

        // Act 2: Buyer confirms the recorded weighing
        currentUserServiceMock.Setup(u => u.UserId).Returns(_fixture.SeedBuyer.UserId);
        currentUserServiceMock.Setup(u => u.Role).Returns(UserRole.Buyer);

        var confirmResult = await weighingService.ConfirmWeighingAsync(recordResult.Data.Id, true);

        // Assert 2: Jointly confirmed! Order's VerifiedNetQuantityKg is updated
        confirmResult.Succeeded.Should().BeTrue();
        var confirmedRecord = await weighingRepo.GetByIdAsync(recordResult.Data.Id);
        confirmedRecord!.Status.Should().Be(WeighingStatus.Confirmed);

        var finalOrder = await orderRepo.GetByIdAsync(order.Id);
        finalOrder!.VerifiedNetQuantityKg.Should().Be(2000m);
        finalOrder.Status.Should().Be(OrderStatus.PaymentPending);
    }

    [Fact]
    public async Task PaymentVerification_RecalculatesVerifiedPaidAmount_AndCompletesOrderWhenFullyPaid()
    {
        // Arrange
        var context = _fixture.Context;
        var unitOfWork = new UnitOfWork(context);
        var orderRepo = new OrderRepository(context);
        var paymentRepo = new PaymentRecordRepository(context);
        var auditLoggerMock = new Mock<IAuditLogger>();
        var notificationMock = new Mock<INotificationService>();

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(u => u.UserId).Returns(_fixture.SeedBuyer.UserId);
        currentUserServiceMock.Setup(u => u.Role).Returns(UserRole.Buyer);

        var paymentService = new PaymentService(
            paymentRepo,
            orderRepo,
            unitOfWork,
            currentUserServiceMock.Object,
            auditLoggerMock.Object,
            notificationMock.Object);

        // Create a distinct offer for this order
        var offer2 = new BuyerOffer
        {
            OfferCode = "OFR-2026-PAYTEST-01",
            HarvestLotId = _fixture.SeedLot.Id,
            BuyerProfileId = _fixture.SeedBuyer.Id,
            OfferedPricePerKg = 35m,
            OfferedQuantityKg = 2000m,
            ExpiryDateUtc = DateTime.UtcNow.AddDays(7),
            Status = OfferStatus.Accepted
        };
        context.BuyerOffers.Add(offer2);
        await unitOfWork.SaveChangesAsync();

        // Seed order: Net payable = ৳ 70,000
        var order = new Order
        {
            OrderNumber = "KB-2026-TEST-P01",
            IdempotencyKey = Guid.NewGuid().ToString(),
            HarvestLotId = _fixture.SeedLot.Id,
            BuyerOfferId = offer2.Id,
            FarmerProfileId = _fixture.SeedFarmer.Id,
            BuyerProfileId = _fixture.SeedBuyer.Id,
            AgreedPricePerKg = 35.00m,
            AgreedQuantityKg = 2000.00m,
            VerifiedNetQuantityKg = 2000.00m,
            VerifiedPaidAmount = 0m,
            Status = OrderStatus.PaymentPending
        };
        await orderRepo.AddAsync(order);
        await unitOfWork.SaveChangesAsync();

        order.NetPayableToFarmer.Should().Be(70000m);
        order.OutstandingBalance.Should().Be(70000m);

        // Act 1: Buyer submits payment of ৳ 70,000 via bKash
        var recordPaymentDto = new RecordPaymentDto
        {
            OrderId = order.Id,
            Amount = 70000m,
            Method = PaymentMethod.bKash,
            TransactionReference = "BKASH-TEST-998877",
            SenderAccountOrPhone = "01811000002",
            ReceiverAccountOrPhone = "01711000001"
        };
        var submitResult = await paymentService.RecordDirectPaymentAsync(recordPaymentDto);

        // Assert 1: Payment recorded as PendingVerification, balance still ৳ 70,000!
        submitResult.Succeeded.Should().BeTrue();
        submitResult.Data!.Status.Should().Be(PaymentStatus.PendingVerification);

        var orderAfterSubmit = await orderRepo.GetByIdAsync(order.Id);
        orderAfterSubmit!.VerifiedPaidAmount.Should().Be(0m);
        orderAfterSubmit.OutstandingBalance.Should().Be(70000m);

        // Act 2: Farmer verifies and approves the payment
        currentUserServiceMock.Setup(u => u.UserId).Returns(_fixture.SeedFarmer.UserId);
        currentUserServiceMock.Setup(u => u.Role).Returns(UserRole.Farmer);

        var verifyResult = await paymentService.VerifyPaymentAsync(new VerifyPaymentDto
        {
            PaymentRecordId = submitResult.Data.Id,
            IsApproved = true,
            VerificationNotes = "বিকাশ স্টেটমেন্ট যাচাই করে ৳ ৭০,০০০ জমা পাওয়া গেছে।"
        });

        // Assert 2: Balance reduced to 0, order automatically completed!
        verifyResult.Succeeded.Should().BeTrue();

        var orderAfterVerify = await orderRepo.GetByIdAsync(order.Id);
        orderAfterVerify!.VerifiedPaidAmount.Should().Be(70000m);
        orderAfterVerify.OutstandingBalance.Should().Be(0m);
        orderAfterVerify.Status.Should().Be(OrderStatus.Completed);
        orderAfterVerify.CompletedAtUtc.Should().NotBeNull();
    }
}
