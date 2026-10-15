using System;
using System.Collections.Generic;
using System.Globalization;
using KrishiBondhu.Application.Interfaces.Services;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Infrastructure.Services;

public class LocalizationService : ILocalizationService
{
    public string CurrentLanguage { get; private set; } = "bn";

    public void SetLanguage(string languageCode)
    {
        CurrentLanguage = (languageCode?.ToLowerInvariant() == "en") ? "en" : "bn";
    }

    private static readonly Dictionary<string, (string bn, string en)> Dictionary = new()
    {
        ["PlatformTitle"] = ("কৃষিবন্ধু ডিসিশন ইঞ্জিন", "KrishiBondhu Decision Engine"),
        ["PlatformSubtitle"] = ("নিরাপদ কৃষি বাণিজ্য ও কৃষকের সঠিক মূল্য নির্ধারণের আধুনিক প্ল্যাটফর্ম", "Secure Agricultural Trading & Farmer Decision Support Platform"),
        ["Dashboard"] = ("ড্যাশবোর্ড", "Dashboard"),
        ["HarvestLots"] = ("ফসলের লটসমূহ", "Harvest Lots"),
        ["MyLots"] = ("আমার ফসলের লট", "My Lots"),
        ["AddLot"] = ("নতুন ফসলের লট যুক্ত করুন", "Add Harvest Lot"),
        ["Offers"] = ("দরপ্রস্তাবসমূহ", "Offers"),
        ["Orders"] = ("লেনদেন ও অর্ডার", "Orders"),
        ["Weighing"] = ("ওজন পরিমাপ ও যাচাই", "Weighing Verification"),
        ["Payments"] = ("পেমেন্ট ও হিসাব", "Payments & Balance"),
        ["Disputes"] = ("বিরোধ ও সালিশি", "Dispute Resolution"),
        ["BuyerDemands"] = ("ক্রেতার চাহিদা", "Buyer Demands"),
        ["GrossSaleValue"] = ("মোট বিক্রয় মূল্য", "Gross Sale Value"),
        ["NetProceeds"] = ("আনুমানিক নিট বিক্রয় মূল্য", "Estimated Net Proceeds"),
        ["Deductions"] = ("মোট প্রযোজ্য কর্তন", "Total Deductions"),
        ["TransportCost"] = ("পরিবহন খরচ", "Transport Cost"),
        ["WeighingLabor"] = ("ওজন ও কুলি খরচ", "Weighing & Labor"),
        ["PackagingCost"] = ("প্যাকেজিং খরচ", "Packaging Cost"),
        ["PlatformFee"] = ("প্ল্যাটফর্ম সেবা ফি", "Platform Fee"),
        ["OutstandingBalance"] = ("বকেয়া পাওনা", "Outstanding Balance"),
        ["VerifiedPaidAmount"] = ("যাচাইকৃত প্রাপ্ত টাকা", "Verified Paid Amount"),
        ["NetWeight"] = ("প্রকৃত নিট ওজন", "Net Weight"),
        ["GrossWeight"] = ("মোট ওজন (Gross)", "Gross Weight"),
        ["TareWeight"] = ("বস্তা/পাত্রের ওজন (Tare)", "Tare Weight"),
        ["Save"] = ("সংরক্ষণ করুন", "Save"),
        ["Cancel"] = ("বাতিল", "Cancel"),
        ["Confirm"] = ("নিশ্চিত করুন", "Confirm"),
        ["AcceptOffer"] = ("দরপ্রস্তাব গ্রহণ করুন", "Accept Offer"),
        ["RejectOffer"] = ("প্রত্যাখ্যান করুন", "Reject Offer"),
        ["RaiseDispute"] = ("বিরোধ উত্থাপন করুন", "Raise Dispute"),
        ["ResolveDispute"] = ("বিরোধ মীমাংসা করুন", "Resolve Dispute"),
        ["RecordPayment"] = ("পেমেন্ট জমা দিন", "Record Payment"),
        ["VerifyPayment"] = ("পেমেন্ট যাচাই করুন", "Verify Payment"),
        ["Login"] = ("লগইন", "Login"),
        ["Register"] = ("নিবন্ধন", "Register"),
        ["Logout"] = ("লগআউট", "Logout")
    };

    public string Get(string key)
    {
        if (Dictionary.TryGetValue(key, out var translation))
        {
            return CurrentLanguage == "en" ? translation.en : translation.bn;
        }
        return key;
    }

    public string FormatCurrency(decimal amount)
    {
        var formatted = string.Format(CultureInfo.InvariantCulture, "{0:N2}", amount);
        if (CurrentLanguage == "bn")
        {
            return $"৳ {ConvertToBanglaNumerals(formatted)}";
        }
        return $"৳ {formatted}";
    }

    public string FormatWeight(decimal weight, WeightUnit unit)
    {
        var formatted = string.Format(CultureInfo.InvariantCulture, "{0:N2}", weight);
        if (unit == WeightUnit.Mon)
        {
            var kgEquiv = string.Format(CultureInfo.InvariantCulture, "{0:N2}", weight * 40m);
            if (CurrentLanguage == "bn")
            {
                return $"{ConvertToBanglaNumerals(formatted)} মণ ({ConvertToBanglaNumerals(kgEquiv)} কেজি)";
            }
            return $"{formatted} Mon ({kgEquiv} Kg)";
        }
        else
        {
            if (CurrentLanguage == "bn")
            {
                return $"{ConvertToBanglaNumerals(formatted)} কেজি";
            }
            return $"{formatted} Kg";
        }
    }

    public static string ConvertToBanglaNumerals(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var chars = input.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = chars[i] switch
            {
                '0' => '০',
                '1' => '১',
                '2' => '২',
                '3' => '৩',
                '4' => '৪',
                '5' => '৫',
                '6' => '৬',
                '7' => '৭',
                '8' => '৮',
                '9' => '৯',
                _ => chars[i]
            };
        }
        return new string(chars);
    }
}
