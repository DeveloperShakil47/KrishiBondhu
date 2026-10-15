using System.ComponentModel.DataAnnotations;

namespace KrishiBondhu.Domain.Enums;

public enum UserRole
{
    [Display(Name = "প্রশাসক (Administrator)")]
    Administrator = 1,

    [Display(Name = "কৃষক (Farmer)")]
    Farmer = 2,

    [Display(Name = "ক্রেতা / ব্যবসায়ী (Buyer)")]
    Buyer = 3,

    [Display(Name = "ওজন পরিমাপক (Weighing Operator)")]
    WeighingOperator = 4,

    [Display(Name = "সহায়তা কর্মকর্তা (Support Agent)")]
    SupportAgent = 5
}

public enum QualityGrade
{
    [Display(Name = "প্রিমিয়াম (Premium / A+)")]
    Premium = 1,

    [Display(Name = "গ্রেড এ (Grade A)")]
    GradeA = 2,

    [Display(Name = "গ্রেড বি (Grade B)")]
    GradeB = 3,

    [Display(Name = "সাধারণ (Standard / Grade C)")]
    Standard = 4
}

public enum WeightUnit
{
    [Display(Name = "কেজি (Kilogram)")]
    Kg = 1,

    [Display(Name = "মণ (Maund / 40 Kg)")]
    Mon = 40
}

public enum LotStatus
{
    [Display(Name = "খসড়া (Draft)")]
    Draft = 1,

    [Display(Name = "তালিকাভুক্ত (Active Listed)")]
    ActiveListed = 2,

    [Display(Name = "দরপ্রস্তাব চলছে (Under Offer)")]
    UnderOffer = 3,

    [Display(Name = "বিক্রিত (Sold)")]
    Sold = 4,

    [Display(Name = "বাতিল (Cancelled)")]
    Cancelled = 5
}

public enum OfferStatus
{
    [Display(Name = "প্রস্তাবিত (Submitted)")]
    Submitted = 1,

    [Display(Name = "বিবেচনাধীন (Under Review)")]
    UnderReview = 2,

    [Display(Name = "গৃহীত (Accepted)")]
    Accepted = 3,

    [Display(Name = "প্রত্যাখ্যাত (Rejected)")]
    Rejected = 4,

    [Display(Name = "মেয়াদোত্তীর্ণ (Expired)")]
    Expired = 5,

    [Display(Name = "পাল্টা প্রস্তাব (Counter Offered)")]
    CounterOffered = 6
}

public enum OrderStatus
{
    [Display(Name = "খসড়া (Draft)")]
    Draft = 1,

    [Display(Name = "প্রস্তাব গৃহীত (Offer Accepted)")]
    OfferAccepted = 2,

    [Display(Name = "ডেলিভারির অপেক্ষায় (Awaiting Delivery)")]
    AwaitingDelivery = 3,

    [Display(Name = "ডেলিভারি সম্পন্ন ও নিশ্চিতকরণের অপেক্ষায় (Delivered Pending Confirmation)")]
    DeliveredPendingConfirmation = 4,

    [Display(Name = "পেমেন্ট অপেক্ষমাণ (Payment Pending)")]
    PaymentPending = 5,

    [Display(Name = "আংশিক পরিশোধিত (Payment Partially Received)")]
    PaymentPartiallyReceived = 6,

    [Display(Name = "সম্পূর্ণ সম্পন্ন (Completed)")]
    Completed = 7,

    [Display(Name = "বাতিলকৃত (Cancelled)")]
    Cancelled = 8,

    [Display(Name = "বিরোধপূর্ণ (Disputed)")]
    Disputed = 9
}

public enum WeighingStatus
{
    [Display(Name = "রেকর্ডকৃত (Recorded)")]
    Recorded = 1,

    [Display(Name = "কৃষক দ্বারা স্বীকৃত (Farmer Agreed)")]
    FarmerAgreed = 2,

    [Display(Name = "ক্রেতা দ্বারা স্বীকৃত (Buyer Agreed)")]
    BuyerAgreed = 3,

    [Display(Name = "যৌথভাবে নিশ্চিত (Jointly Confirmed)")]
    Confirmed = 4,

    [Display(Name = "বিরোধ উত্থাপিত (Disputed)")]
    Disputed = 5,

    [Display(Name = "সংশোধিত (Corrected)")]
    Corrected = 6
}

public enum PaymentMethod
{
    [Display(Name = "বিকাশ (bKash)")]
    bKash = 1,

    [Display(Name = "নগদ (Nagad)")]
    Nagad = 2,

    [Display(Name = "রকেট (Rocket)")]
    Rocket = 3,

    [Display(Name = "ব্যাংক স্থানান্তর (Bank Transfer)")]
    BankTransfer = 4,

    [Display(Name = "ক্যাশ অন ডেলিভারি (Cash on Delivery)")]
    CashOnDelivery = 5
}

public enum PaymentStatus
{
    [Display(Name = "যাচাইকরণ বাকি (Pending Verification)")]
    PendingVerification = 1,

    [Display(Name = "আংশিক পরিশোধিত (Partially Paid)")]
    PartiallyPaid = 2,

    [Display(Name = "পরিশোধিত ও যাচাইকৃত (Paid & Verified)")]
    Paid = 3,

    [Display(Name = "ব্যর্থ (Failed)")]
    Failed = 4,

    [Display(Name = "প্রত্যাবর্তিত (Reversed)")]
    Reversed = 5,

    [Display(Name = "বিরোধপূর্ণ (Disputed)")]
    Disputed = 6
}

public enum DisputeReason
{
    [Display(Name = "ওজন বা পরিমাপে গরমিল (Quantity Mismatch)")]
    QuantityMismatch = 1,

    [Display(Name = "পণ্যের মানে ঘাটতি (Quality Defect)")]
    QualityDefect = 2,

    [Display(Name = "অননুমোদিত খরচ কর্তন (Price Deduction Dispute)")]
    PriceDeductionDispute = 3,

    [Display(Name = "ডেলিভারিতে অযৌক্তিক বিলম্ব (Delivery Delay)")]
    DeliveryDelay = 4,

    [Display(Name = "পেমেন্ট বকেয়া বা জাল ট্রানজ্যাকশন (Payment Default)")]
    PaymentDefault = 5,

    [Display(Name = "প্যাকেজিং বা পরিবহনে ক্ষতি (Damage in Transit)")]
    PackagingDamage = 6,

    [Display(Name = "অন্যান্য কারণ (Other)")]
    Other = 7
}

public enum DisputeStatus
{
    [Display(Name = "উন্মুক্ত (Open)")]
    Open = 1,

    [Display(Name = "পর্যালোচনা চলছে (Under Review)")]
    UnderReview = 2,

    [Display(Name = "তথ্য প্রয়োজন (Awaiting Information)")]
    AwaitingInformation = 3,

    [Display(Name = "মীমাংসিত (Resolved)")]
    Resolved = 4,

    [Display(Name = "প্রত্যাখ্যাত (Rejected)")]
    Rejected = 5
}

public enum CostType
{
    [Display(Name = "পরিবহন খরচ (Transport Cost)")]
    Transport = 1,

    [Display(Name = "ওজন ও কুলি খরচ (Weighing & Labor)")]
    WeighingAndLabor = 2,

    [Display(Name = "প্যাকেজিং ও বস্তা খরচ (Packaging)")]
    Packaging = 3,

    [Display(Name = "প্ল্যাটফর্ম সেবা ফি (Platform Fee)")]
    PlatformFee = 4,

    [Display(Name = "হ্যান্ডলিং খরচ (Handling)")]
    Handling = 5,

    [Display(Name = "অন্যান্য (Other)")]
    Other = 6
}

public enum BuyerBusinessType
{
    [Display(Name = "আড়তদার / পাইকারি ব্যবসায়ী (Wholesaler / Aratdar)")]
    WholesalerAratdar = 1,

    [Display(Name = "খুচরা বিক্রেতা (Retailer)")]
    Retailer = 2,

    [Display(Name = "খাদ্য প্রক্রিয়াজাতকারী প্রতিষ্ঠান (Food Processor)")]
    FoodProcessor = 3,

    [Display(Name = "রাইস মিল / চাতাল মালিক (Rice Mill Owner)")]
    RiceMillOwner = 4,

    [Display(Name = "রপ্তানিকারক (Exporter)")]
    Exporter = 5,

    [Display(Name = "কৃষি বাণিজ্যিক সংস্থা (Agro Enterprise)")]
    AgroEnterprise = 6
}

public enum MarketType
{
    [Display(Name = "পাইকারি বাজার (Wholesale Market)")]
    Wholesale = 1,

    [Display(Name = "খুচরা বাজার (Retail Market)")]
    Retail = 2,

    [Display(Name = "পাইকারি ও খুচরা উভয় (Wholesale & Retail)")]
    Both = 3
}

public enum PriceVerificationStatus
{
    [Display(Name = "যাচাইহীন / প্রাথমিক তথ্য (Unverified)")]
    Unverified = 1,

    [Display(Name = "যাচাইকৃত / অনুমোদিত (Verified)")]
    Verified = 2,

    [Display(Name = "প্রত্যাখ্যাত / অগ্রহণযোগ্য (Rejected)")]
    Rejected = 3
}

public enum PriceSourceType
{
    [Display(Name = "সরকারি কর্তৃপক্ষ (Government Official - DAM/TCB)")]
    GovernmentOfficial = 1,

    [Display(Name = "মাঠপর্যায়ের বাজার জরিপকারী (Field Market Surveyor)")]
    MarketSurvey = 2,

    [Display(Name = "বাজার ব্যবস্থাপনা কমিটি / আড়তদার সমিতি (Market Committee)")]
    MarketCommittee = 3,

    [Display(Name = "সরাসরি ব্যবসায়ী / পাইকার (Direct Merchant)")]
    DirectTrader = 4,

    [Display(Name = "ভোক্তা / নাগরিক রিপোর্ট (Citizen Report)")]
    CrowdSource = 5
}

public enum ComplaintStatus
{
    [Display(Name = "দাখিলকৃত (Submitted)")]
    Submitted = 1,

    [Display(Name = "পর্যালোচনাধীন (Under Review)")]
    UnderReview = 2,

    [Display(Name = "অতিরিক্ত তথ্য প্রয়োজন (Awaiting Information)")]
    AwaitingInformation = 3,

    [Display(Name = "মীমাংসিত (Resolved)")]
    Resolved = 4,

    [Display(Name = "প্রত্যাখ্যাত (Rejected)")]
    Rejected = 5,

    [Display(Name = "আইনশৃঙ্খলা / ভোক্তা অধিকারে প্রেরিত (Referred to Authority)")]
    ReferredToAuthority = 6
}

public enum StandardCommodityUnit
{
    [Display(Name = "কেজি (Kilogram / Kg)")]
    Kg = 1,

    [Display(Name = "লিটার (Litre)")]
    Litre = 2,

    [Display(Name = "টি / পিস (Piece)")]
    Piece = 3,

    [Display(Name = "মণ (Maund / 40 Kg)")]
    Mon = 4,

    [Display(Name = "গ্রাম (Gram)")]
    Gram = 5,

    [Display(Name = "কুইন্টাল (Quintal / 100 Kg)")]
    Quintal = 6,

    [Display(Name = "হালি (Hali / 4 Pieces)")]
    Hali = 7,

    [Display(Name = "ডজন (Dozen / 12 Pieces)")]
    Dozen = 8
}
