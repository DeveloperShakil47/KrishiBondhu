using System;
using FluentAssertions;
using Xunit;
using KrishiBondhu.Domain.Entities;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.UnitTests;

public class PaymentAndBalanceTests
{
    [Fact]
    public void Order_OutstandingBalance_CorrectlyReflectsVerifiedPaymentsOnly()
    {
        // Arrange: 1000 Kg @ 35 ৳/Kg with 2000 ৳ deductions -> Net Payable = 33,000
        var order = new Order
        {
            AgreedPricePerKg = 35.00m,
            AgreedQuantityKg = 1000m,
            AgreedTransportDeduction = 1500m,
            AgreedPackagingDeduction = 500m,
            VerifiedPaidAmount = 0m
        };

        order.NetPayableToFarmer.Should().Be(33000.00m);
        order.OutstandingBalance.Should().Be(33000.00m);

        // Act: A payment of ৳ 20,000 is verified
        order.VerifiedPaidAmount = 20000.00m;

        // Assert: Balance is reduced to 13,000
        order.OutstandingBalance.Should().Be(13000.00m);

        // Act 2: Additional payment of ৳ 13,000 is verified
        order.VerifiedPaidAmount = 33000.00m;

        // Assert: Fully paid
        order.OutstandingBalance.Should().Be(0.00m);
    }

    [Fact]
    public void PaymentRecord_DefaultStatus_IsPendingVerification()
    {
        // Arrange & Act
        var payment = new PaymentRecord
        {
            OrderId = Guid.NewGuid(),
            Amount = 15000m,
            Method = PaymentMethod.bKash,
            TransactionReference = "8M76B9X21"
        };

        // Assert: Must never be automatically marked Paid or Verified!
        payment.Status.Should().Be(PaymentStatus.PendingVerification);
        payment.VerifiedByUserId.Should().BeNull();
        payment.VerifiedAtUtc.Should().BeNull();
    }
}
