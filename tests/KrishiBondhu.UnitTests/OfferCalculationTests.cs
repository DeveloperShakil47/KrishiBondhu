using System;
using FluentAssertions;
using Xunit;
using KrishiBondhu.Domain.Entities;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.UnitTests;

public class OfferCalculationTests
{
    [Fact]
    public void BuyerOffer_CalculatesGrossValue_AndNetProceedsAccurately()
    {
        // Arrange
        var offer = new BuyerOffer
        {
            OfferedPricePerKg = 35.00m,
            OfferedQuantityKg = 2000.00m,
            TransportCostDeduction = 1500.00m,
            WeighingAndLaborCostDeduction = 600.00m,
            PackagingCostDeduction = 800.00m,
            PlatformFee = 350.00m, // 0.5% of 70,000
            OtherDeductions = 0m
        };

        // Act & Assert
        // Gross = 2,000 * 35 = 70,000
        offer.GrossSaleValue.Should().Be(70000.00m);

        // Deductions = 1500 + 600 + 800 + 350 = 3250
        offer.TotalEstimatedDeductions.Should().Be(3250.00m);

        // Net Proceeds = 70,000 - 3250 = 66,750
        offer.EstimatedNetProceeds.Should().Be(66750.00m);
    }

    [Fact]
    public void DecisionEngine_DistinguishesNetProceeds_FromTrueCropProfit()
    {
        // Arrange
        var offer = new BuyerOffer
        {
            OfferedPricePerKg = 36.00m,
            OfferedQuantityKg = 2000.00m,
            TransportCostDeduction = 1200.00m,
            WeighingAndLaborCostDeduction = 500.00m,
            PackagingCostDeduction = 600.00m,
            PlatformFee = 360.00m
        };

        // Farmer's production cost: ৳ 24.50 per kg (seeds, fertilizers, irrigation, tillage)
        decimal farmerProductionCostPerKg = 24.50m;
        decimal totalProductionCost = farmerProductionCostPerKg * offer.OfferedQuantityKg; // 49,000.00

        // Act
        decimal netSellingProceeds = offer.EstimatedNetProceeds; // Gross (72,000) - Deductions (2,660) = 69,340
        decimal trueCropProfit = netSellingProceeds - totalProductionCost; // 69,340 - 49,000 = 20,340

        // Assert
        offer.GrossSaleValue.Should().Be(72000.00m);
        offer.TotalEstimatedDeductions.Should().Be(2660.00m);
        netSellingProceeds.Should().Be(69340.00m);

        // True crop profit must be strictly less than net selling proceeds
        trueCropProfit.Should().Be(20340.00m);
        trueCropProfit.Should().BeLessThan(netSellingProceeds);
    }

    [Fact]
    public void Order_CalculatesTotalGrossAndNetPayable_BasedOnVerifiedWeight()
    {
        // Arrange
        var order = new Order
        {
            AgreedPricePerKg = 34.00m,
            AgreedQuantityKg = 2000.00m,
            AgreedTransportDeduction = 1000m,
            AgreedWeighingDeduction = 500m,
            AgreedPackagingDeduction = 500m,
            AgreedPlatformFee = 340m,
            VerifiedNetQuantityKg = 1980.00m // Measured at digital scale (20 kg less due to dryage)
        };

        // Act & Assert
        // Effective Quantity is the verified net quantity
        order.EffectiveQuantityKg.Should().Be(1980.00m);

        // Gross Amount = 1980 * 34 = 67,320.00
        order.TotalGrossAmount.Should().Be(67320.00m);

        // Total Deductions = 1000 + 500 + 500 + 340 = 2,340.00
        order.TotalAgreedDeductions.Should().Be(2340.00m);

        // Net Payable to Farmer = 67,320 - 2,340 = 64,980.00
        order.NetPayableToFarmer.Should().Be(64980.00m);
    }
}
