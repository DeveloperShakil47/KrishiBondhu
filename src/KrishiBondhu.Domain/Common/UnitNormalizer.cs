using System;
using KrishiBondhu.Domain.Enums;
using KrishiBondhu.Domain.Exceptions;

namespace KrishiBondhu.Domain.Common;

public enum UnitDimension
{
    Weight,
    Volume,
    Count
}

public static class UnitNormalizer
{
    public const string BaseUnitWeight = "কেজি (Kg)";
    public const string BaseUnitVolume = "লিটার (Litre)";
    public const string BaseUnitCount = "টি / পিস (Piece)";

    public static string CanonicalizeUnit(string rawUnit)
    {
        if (string.IsNullOrWhiteSpace(rawUnit))
            return BaseUnitWeight;

        var clean = rawUnit.Trim().ToLowerInvariant();

        // Weight variants
        if (clean.Contains("কেজি") || clean.Contains("kg") || clean.Contains("kilo") || clean.Contains("কে.জি") || clean.Contains("kilogram"))
            return "কেজি";
        if (clean.Contains("মণ") || clean.Contains("মন") || clean.Contains("mon") || clean.Contains("maund"))
            return "মণ";
        if (clean.Contains("গ্রাম") || clean == "g" || clean == "gm" || clean == "gram")
            return "গ্রাম";
        if (clean.Contains("কুইন্টাল") || clean.Contains("quintal"))
            return "কুইন্টাল";
        if (clean.Contains("টন") || clean.Contains("ton"))
            return "টন";

        // Volume variants
        if (clean.Contains("লিটার") || clean.Contains("litre") || clean.Contains("liter") || clean == "l" || clean == "ltr")
            return "লিটার";
        if (clean.Contains("মিলি") || clean.Contains("ml") || clean.Contains("millilitre"))
            return "মিলি";

        // Count variants
        if (clean.Contains("হালি") || clean.Contains("hali"))
            return "হালি";
        if (clean.Contains("ডজন") || clean.Contains("dozen"))
            return "ডজন";
        if (clean.Contains("টি") || clean.Contains("পিস") || clean.Contains("piece") || clean.Contains("pc") || clean.Contains("দানা"))
            return "টি";

        // Default to Weight (Kg)
        return "কেজি";
    }

    public static UnitDimension GetDimension(string unit)
    {
        var canonical = CanonicalizeUnit(unit);
        return canonical switch
        {
            "কেজি" or "মণ" or "গ্রাম" or "কুইন্টাল" or "টন" => UnitDimension.Weight,
            "লিটার" or "মিলি" => UnitDimension.Volume,
            "টি" or "হালি" or "ডজন" => UnitDimension.Count,
            _ => UnitDimension.Weight
        };
    }

    public static string GetStandardBaseUnit(UnitDimension dimension)
    {
        return dimension switch
        {
            UnitDimension.Weight => "কেজি",
            UnitDimension.Volume => "লিটার",
            UnitDimension.Count => "টি",
            _ => "কেজি"
        };
    }

    public static decimal GetMultiplierToBase(string unit)
    {
        var canonical = CanonicalizeUnit(unit);
        return canonical switch
        {
            // Weight to KG
            "কেজি" => 1.0m,
            "মণ" => 40.0m,
            "গ্রাম" => 0.001m,
            "কুইন্টাল" => 100.0m,
            "টন" => 1000.0m,

            // Volume to Litre
            "লিটার" => 1.0m,
            "মিলি" => 0.001m,

            // Count to Pieces
            "টি" => 1.0m,
            "হালি" => 4.0m,
            "ডজন" => 12.0m,

            _ => 1.0m
        };
    }

    public static bool AreCompatible(string unitA, string unitB)
    {
        return GetDimension(unitA) == GetDimension(unitB);
    }

    public static (decimal NormalizedQuantity, string BaseUnit) NormalizeQuantity(decimal quantity, string unit)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "পরিমাণ অবশ্যই শূন্যের চেয়ে বেশি হতে হবে।");

        var dim = GetDimension(unit);
        var baseUnit = GetStandardBaseUnit(dim);
        var multiplier = GetMultiplierToBase(unit);

        var normalized = Math.Round(quantity * multiplier, 4);
        return (normalized, baseUnit);
    }

    public static (decimal NormalizedPricePerUnit, string BaseUnit) CalculateNormalizedPrice(decimal totalPrice, decimal quantity, string unit)
    {
        if (totalPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(totalPrice), "মূল্য কখনো ঋণাত্মক হতে পারে না।");

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "পরিমাণ অবশ্যই শূন্যের চেয়ে বেশি হতে হবে।");

        var (normalizedQty, baseUnit) = NormalizeQuantity(quantity, unit);
        if (normalizedQty == 0)
            throw new InvalidOperationException("নর্মালাইজড পরিমাণ শূন্য হতে পারে না।");

        var pricePerUnit = Math.Round(totalPrice / normalizedQty, 2);
        return (pricePerUnit, baseUnit);
    }
}
