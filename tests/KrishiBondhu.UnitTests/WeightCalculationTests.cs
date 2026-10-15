using System;
using FluentAssertions;
using Xunit;
using KrishiBondhu.Domain.Entities;
using KrishiBondhu.Domain.Enums;
using KrishiBondhu.Domain.Exceptions;

namespace KrishiBondhu.UnitTests;

public class WeightCalculationTests
{
    [Fact]
    public void Create_WithValidGrossAndTare_CalculatesAccurateNetWeight()
    {
        // Arrange
        decimal gross = 2050.50m;
        decimal tare = 50.25m;
        var orderId = Guid.NewGuid();

        // Act
        var record = WeighingRecord.Create(
            orderId,
            gross,
            tare,
            WeightUnit.Kg,
            "মহাস্থান হাট ওজন কেন্দ্র",
            "বগুড়া",
            "শিবগঞ্জ");

        // Assert
        record.NetWeight.Should().Be(2000.25m);
        record.NetWeightInKg.Should().Be(2000.25m);
        record.Status.Should().Be(WeighingStatus.Recorded);
    }

    [Theory]
    [InlineData(1, 40)]
    [InlineData(5, 200)]
    [InlineData(50, 2000)]
    [InlineData(25.5, 1020)]
    public void HarvestLot_QuantityInKg_ConvertsMonToKgCorrectly(decimal mon, decimal expectedKg)
    {
        // Arrange & Act
        var lot = new HarvestLot
        {
            Quantity = mon,
            Unit = WeightUnit.Mon
        };

        // Assert
        lot.QuantityInKg.Should().Be(expectedKg);
    }

    [Fact]
    public void Create_WhenGrossWeightIsZeroOrNegative_ThrowsInvalidWeightException()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act & Assert
        Action actZero = () => WeighingRecord.Create(orderId, 0, 10, WeightUnit.Kg, "পয়েন্ট", "বগুড়া", "শিবগঞ্জ");
        actZero.Should().Throw<InvalidWeightException>()
            .WithMessage("*শূন্যের চেয়ে বেশি হতে হবে*");

        Action actNegative = () => WeighingRecord.Create(orderId, -100, 10, WeightUnit.Kg, "পয়েন্ট", "বগুড়া", "শিবগঞ্জ");
        actNegative.Should().Throw<InvalidWeightException>();
    }

    [Fact]
    public void Create_WhenTareWeightIsNegative_ThrowsInvalidWeightException()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act & Assert
        Action act = () => WeighingRecord.Create(orderId, 1000, -5, WeightUnit.Kg, "পয়েন্ট", "বগুড়া", "শিবগঞ্জ");
        act.Should().Throw<InvalidWeightException>()
            .WithMessage("*ঋণাত্মক হতে পারে না*");
    }

    [Fact]
    public void Create_WhenGrossWeightIsLessThanOrEqualToTareWeight_ThrowsInvalidWeightException()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act & Assert (Gross equal to Tare)
        Action actEqual = () => WeighingRecord.Create(orderId, 50, 50, WeightUnit.Kg, "পয়েন্ট", "বগুড়া", "শিবগঞ্জ");
        actEqual.Should().Throw<InvalidWeightException>()
            .WithMessage("*Gross must exceed Tare*");

        // Act & Assert (Gross less than Tare)
        Action actLess = () => WeighingRecord.Create(orderId, 40, 50, WeightUnit.Kg, "পয়েন্ট", "বগুড়া", "শিবগঞ্জ");
        actLess.Should().Throw<InvalidWeightException>();
    }
}
