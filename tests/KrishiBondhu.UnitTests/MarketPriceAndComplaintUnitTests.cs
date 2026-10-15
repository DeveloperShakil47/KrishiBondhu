using System;
using KrishiBondhu.Domain.Common;
using KrishiBondhu.Domain.Entities;
using KrishiBondhu.Domain.Enums;
using KrishiBondhu.Infrastructure.Services;
using Xunit;

namespace KrishiBondhu.UnitTests;

public class MarketPriceAndComplaintUnitTests
{
    [Fact]
    public void UnitNormalizer_NormalizesMaundToKilograms_Correctly()
    {
        // 1 Mon = 40 Kg
        var (normalizedQty, baseUnit) = UnitNormalizer.NormalizeQuantity(2.5m, "মণ");

        Assert.Equal(100.0m, normalizedQty);
        Assert.Equal("কেজি", baseUnit);
    }

    [Fact]
    public void UnitNormalizer_NormalizesQuintalAndGramsToKilograms()
    {
        // 1 Quintal = 100 Kg
        var (qQty, qUnit) = UnitNormalizer.NormalizeQuantity(1.5m, "কুইন্টাল");
        Assert.Equal(150.0m, qQty);
        Assert.Equal("কেজি", qUnit);

        // 500 Gram = 0.5 Kg
        var (gQty, gUnit) = UnitNormalizer.NormalizeQuantity(500m, "গ্রাম");
        Assert.Equal(0.5m, gQty);
        Assert.Equal("কেজি", gUnit);
    }

    [Fact]
    public void UnitNormalizer_NormalizesHaliAndDozenToPieces()
    {
        // 5 Hali = 20 Pieces
        var (hQty, hUnit) = UnitNormalizer.NormalizeQuantity(5m, "হালি");
        Assert.Equal(20.0m, hQty);
        Assert.Equal("টি", hUnit);

        // 2 Dozen = 24 Pieces
        var (dQty, dUnit) = UnitNormalizer.NormalizeQuantity(2m, "ডজন");
        Assert.Equal(24.0m, dQty);
        Assert.Equal("টি", dUnit);
    }

    [Fact]
    public void UnitNormalizer_DetectsIncompatibleDimensions()
    {
        // Kg (Weight) is incompatible with Litre (Volume)
        Assert.False(UnitNormalizer.AreCompatible("কেজি", "লিটার"));

        // Mon (Weight) is incompatible with Piece (Count)
        Assert.False(UnitNormalizer.AreCompatible("মণ", "টি"));

        // Mon (Weight) IS compatible with Kg (Weight)
        Assert.True(UnitNormalizer.AreCompatible("মণ", "কেজি"));

        // Hali (Count) IS compatible with Dozen (Count)
        Assert.True(UnitNormalizer.AreCompatible("হালি", "ডজন"));
    }

    [Fact]
    public void UnitNormalizer_CalculatesPricePerNormalizedUnit_Accurately()
    {
        // 1 Mon purchased for 1000 BDT -> Price per Kg should be 1000 / 40 = 25 BDT/Kg
        var (pricePerKg, unit) = UnitNormalizer.CalculateNormalizedPrice(1000m, 1m, "মণ");
        Assert.Equal(25.00m, pricePerKg);
        Assert.Equal("কেজি", unit);

        // 500 Gram purchased for 40 BDT -> Price per Kg should be 40 / 0.5 = 80 BDT/Kg
        var (pricePerKg2, _) = UnitNormalizer.CalculateNormalizedPrice(40m, 500m, "গ্রাম");
        Assert.Equal(80.00m, pricePerKg2);
    }

    [Fact]
    public void OfficialPriceDirective_Applicability_ValidatesDateRangeAndGeography()
    {
        var cropId = Guid.NewGuid();
        var directive = new OfficialPriceDirective
        {
            DirectiveNumber = "DNCRP-2026-TEST",
            CropId = cropId,
            MarketType = MarketType.Both,
            ApplicableDistrict = "ঢাকা (Dhaka)",
            ApplicableUpazila = null, // Nationwide in Dhaka
            EffectiveFromUtc = DateTime.UtcNow.AddDays(-10),
            EffectiveToUtc = DateTime.UtcNow.AddDays(10),
            LegalMaxPrice = 30.0m,
            StandardUnit = "কেজি",
            VerificationStatus = PriceVerificationStatus.Verified,
            IsActive = true
        };

        // Matching transaction
        var isApplicable = directive.IsApplicable(cropId, MarketType.Retail, "ঢাকা (Dhaka)", "তেজগাঁও", DateTime.UtcNow, "কেজি");
        Assert.True(isApplicable);

        // Wrong District -> Should not apply
        var wrongDistrict = directive.IsApplicable(cropId, MarketType.Retail, "বগুড়া (Bogura)", "শিবগঞ্জ", DateTime.UtcNow, "কেজি");
        Assert.False(wrongDistrict);

        // Expired Date -> Should not apply
        var expired = directive.IsApplicable(cropId, MarketType.Retail, "ঢাকা (Dhaka)", "তেজগাঁও", DateTime.UtcNow.AddDays(20), "কেজি");
        Assert.False(expired);

        // Incompatible Unit (Litre for Kg directive) -> Should not apply
        var wrongUnit = directive.IsApplicable(cropId, MarketType.Retail, "ঢাকা (Dhaka)", "তেজগাঁও", DateTime.UtcNow, "লিটার");
        Assert.False(wrongUnit);
    }

    [Fact]
    public void FileStorageService_ValidatesFiles_AccordingToSizeAndTypeRules()
    {
        var storage = new LocalFileStorageService();

        // Valid JPG
        var (validJpg, _) = storage.ValidateFile("receipt.jpg", 1024 * 500, "image/jpeg");
        Assert.True(validJpg);

        // Valid PNG
        var (validPng, _) = storage.ValidateFile("memo.png", 1024 * 1024, "image/png");
        Assert.True(validPng);

        // Invalid executable extension
        var (invalidExe, exeMsg) = storage.ValidateFile("script.exe", 1024, "application/x-msdownload");
        Assert.False(invalidExe);
        Assert.NotNull(exeMsg);

        // Invalid file size > 10MB
        var (tooLarge, sizeMsg) = storage.ValidateFile("large.jpg", 12 * 1024 * 1024, "image/jpeg");
        Assert.False(tooLarge);
        Assert.Contains("১০ মেগাবাইট", sizeMsg);
    }
}
