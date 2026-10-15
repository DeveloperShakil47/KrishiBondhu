using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using KrishiBondhu.Domain.Entities;
using KrishiBondhu.Domain.Enums;
using KrishiBondhu.Infrastructure.Identity;

namespace KrishiBondhu.Infrastructure.Data;

public class DbInitializer
{
    private readonly KrishiBondhuDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(
        KrishiBondhuDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IConfiguration configuration,
        ILogger<DbInitializer> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            if (_context.Database.IsRelational())
            {
                await _context.Database.MigrateAsync();
            }
            else
            {
                await _context.Database.EnsureCreatedAsync();
            }

            await SeedRolesAsync();
            await SeedAdminAsync();
            await SeedCropsAsync();
            await SeedSampleDataAsync();
            await SeedMarketIntelligenceAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during database initialization and seeding.");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        foreach (UserRole role in Enum.GetValues<UserRole>())
        {
            var roleName = role.ToString();
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }
    }

    private async Task SeedAdminAsync()
    {
        var adminEmail = _configuration["SeedSettings:DefaultAdminEmail"] ?? "admin@krishibondhu.gov.bd";
        var adminPassword = _configuration["SeedSettings:DefaultAdminPassword"] ?? "AdminSecurePassword@2026!";

        var adminUser = await _userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "admin",
                Email = adminEmail,
                PhoneNumber = "01700000000",
                FullName = "সিস্টেম প্রশাসক (System Admin)",
                Role = UserRole.Administrator,
                District = "ঢাকা (Dhaka)",
                Upazila = "তেজগাঁও (Tejgaon)",
                IsVerified = true,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, UserRole.Administrator.ToString());
            }
        }
    }

    private async Task SeedCropsAsync()
    {
        if (await _context.Crops.AnyAsync()) return;

        var crops = new[]
        {
            new Crop
            {
                NameBn = "বোরো ধান (ব্রি ধান-২৮)",
                NameEn = "Boro Paddy (BRRI Dhan 28)",
                Category = "খাদ্যশস্য",
                DescriptionBn = "উচ্চ ফলনশীল বোরো মৌসুমের চিকন চালের ধান। রান্নায় সুস্বাদু ও বাজারে প্রচুর চাহিদা।",
                StandardUnit = "কেজি",
                TypicalMinPricePerKg = 32.0m,
                TypicalMaxPricePerKg = 36.0m,
                IconEmoji = "🌾"
            },
            new Crop
            {
                NameBn = "আমন ধান (নাজিরশাইল)",
                NameEn = "Aman Paddy (Najirshail)",
                Category = "খাদ্যশস্য",
                DescriptionBn = "বাংলাদেশের ঐতিহ্যবাহী প্রিমিয়াম কোয়ালিটি সুগন্ধি আমন ধান।",
                StandardUnit = "কেজি",
                TypicalMinPricePerKg = 38.0m,
                TypicalMaxPricePerKg = 44.0m,
                IconEmoji = "🌾"
            },
            new Crop
            {
                NameBn = "আলু (ডায়মন্ড)",
                NameEn = "Potato (Diamant)",
                Category = "শাকসবজি",
                DescriptionBn = "উত্তরাঞ্চলের সর্বাধিক উৎপাদিত গোল সাদা আলু। সংরক্ষণ ক্ষমতা অত্যন্ত ভালো।",
                StandardUnit = "কেজি",
                TypicalMinPricePerKg = 22.0m,
                TypicalMaxPricePerKg = 28.0m,
                IconEmoji = "🥔"
            },
            new Crop
            {
                NameBn = "পেঁয়াজ (তাহেরপুরী)",
                NameEn = "Onion (Taherpuri)",
                Category = "মসলা",
                DescriptionBn = "দেশি খাঁটি তাহেরপুরী জাতের লালচে ঝাঁজালো ও দীর্ঘমেয়াদী সংরক্ষণযোগ্য পেঁয়াজ।",
                StandardUnit = "কেজি",
                TypicalMinPricePerKg = 60.0m,
                TypicalMaxPricePerKg = 75.0m,
                IconEmoji = "🧅"
            },
            new Crop
            {
                NameBn = "তোষা পাট (সোনালী আঁশ)",
                NameEn = "Tossha Jute (Golden Fiber)",
                Category = "আঁশজাতীয়",
                DescriptionBn = "উন্নত মানের লম্বা চকচকে সোনালী আঁশের তোষা জাতের পাট।",
                StandardUnit = "কেজি",
                TypicalMinPricePerKg = 85.0m,
                TypicalMaxPricePerKg = 98.0m,
                IconEmoji = "🌿"
            },
            new Crop
            {
                NameBn = "গম (বারি গম-৩৩)",
                NameEn = "Wheat (BARI Gom-33)",
                Category = "খাদ্যশস্য",
                DescriptionBn = "ব্লাস্ট রোগ প্রতিরোধী ও জিঙ্ক সমৃদ্ধ উচ্চ পুষ্টিমানের লালচে দানা গম।",
                StandardUnit = "কেজি",
                TypicalMinPricePerKg = 34.0m,
                TypicalMaxPricePerKg = 40.0m,
                IconEmoji = "🌾"
            },
            new Crop
            {
                NameBn = "সরিষা (বারি সরিষা-১৪)",
                NameEn = "Mustard (BARI Sharisha-14)",
                Category = "তৈলবীজ",
                DescriptionBn = "স্বল্পমেয়াদী ও সর্বোচ্চ তেল ধারণকারী হলুদ রঙের সরিষা বীজ।",
                StandardUnit = "কেজি",
                TypicalMinPricePerKg = 78.0m,
                TypicalMaxPricePerKg = 90.0m,
                IconEmoji = "🌼"
            }
        };

        await _context.Crops.AddRangeAsync(crops);
        await _context.SaveChangesAsync();
    }

    private async Task SeedSampleDataAsync()
    {
        if (await _context.FarmerProfiles.AnyAsync()) return;

        var defaultPassword = "KrishiPassword@2026!";

        // 1. Farmer User
        var farmerUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "01711000001",
            PhoneNumber = "01711000001",
            Email = "farmer1@krishibondhu.bd",
            FullName = "মো. আবুল হোসেন (কৃষক)",
            Role = UserRole.Farmer,
            District = "বগুড়া (Bogura)",
            Upazila = "শিবগঞ্জ (Shibganj)",
            Union = "মহাস্থান (Mahasthangarh)",
            IsVerified = true,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        await _userManager.CreateAsync(farmerUser, defaultPassword);
        await _userManager.AddToRoleAsync(farmerUser, UserRole.Farmer.ToString());

        var farmerProfile = new FarmerProfile
        {
            UserId = farmerUser.Id,
            FullNameBn = "মো. আবুল হোসেন",
            FullNameEn = "Md. Abul Hossain",
            PhoneNumber = "01711000001",
            NationalIdNumber = "19851018820000045",
            IsNidVerified = true,
            District = "বগুড়া (Bogura)",
            Upazila = "শিবগঞ্জ (Shibganj)",
            Union = "মহাস্থান (Mahasthangarh)",
            VillageOrRoad = "চককানাইপাড়া",
            TotalLandAreaAcres = 4.5m,
            FarmingExperienceYears = 18,
            BankAccountNumber = "102155009876",
            BankName = "সোনালী ব্যাংক পিএলসি",
            BkashNumber = "01711000001",
            NagadNumber = "01711000001",
            IsActive = true
        };
        await _context.FarmerProfiles.AddAsync(farmerProfile);

        // 2. Buyer User
        var buyerUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "01811000002",
            PhoneNumber = "01811000002",
            Email = "buyer1@uttaraagro.bd",
            FullName = "হাজী দেলোয়ার জাহান (ব্যবসায়ী)",
            Role = UserRole.Buyer,
            District = "বগুড়া (Bogura)",
            Upazila = "বগুড়া সদর (Bogura Sadar)",
            IsVerified = true,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        await _userManager.CreateAsync(buyerUser, defaultPassword);
        await _userManager.AddToRoleAsync(buyerUser, UserRole.Buyer.ToString());

        var buyerProfile = new BuyerProfile
        {
            UserId = buyerUser.Id,
            BusinessName = "মেসার্স উত্তরা অ্যাগ্রো ট্রেডার্স",
            ContactPersonName = "হাজী দেলোয়ার জাহান",
            PhoneNumber = "01811000002",
            Email = "buyer1@uttaraagro.bd",
            BusinessType = BuyerBusinessType.WholesalerAratdar,
            TradeLicenseNumber = "TL-BOG-2024-8902",
            IsTradeLicenseVerified = true,
            District = "বগুড়া (Bogura)",
            Upazila = "বগুড়া সদর",
            WarehouseAddress = "রাজাবাজার আড়ত পট্টি, বগুড়া",
            Rating = 4.9m,
            CompletedTransactionsCount = 42,
            IsActive = true
        };
        await _context.BuyerProfiles.AddAsync(buyerProfile);

        // 3. Weighing Operator User
        var operatorUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "01911000003",
            PhoneNumber = "01911000003",
            Email = "operator1@krishibondhu.bd",
            FullName = "মো. রফিকুল ইসলাম (ওজন পরিমাপক)",
            Role = UserRole.WeighingOperator,
            District = "বগুড়া (Bogura)",
            Upazila = "শিবগঞ্জ (Shibganj)",
            IsVerified = true,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        await _userManager.CreateAsync(operatorUser, defaultPassword);
        await _userManager.AddToRoleAsync(operatorUser, UserRole.WeighingOperator.ToString());

        // 4. Support Agent User
        var supportUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "01611000004",
            PhoneNumber = "01611000004",
            Email = "support1@krishibondhu.bd",
            FullName = "সুমাইয়া আক্তার (সহায়তা কর্মকর্তা)",
            Role = UserRole.SupportAgent,
            District = "ঢাকা (Dhaka)",
            Upazila = "ধানমন্ডি (Dhanmondi)",
            IsVerified = true,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        await _userManager.CreateAsync(supportUser, defaultPassword);
        await _userManager.AddToRoleAsync(supportUser, UserRole.SupportAgent.ToString());

        await _context.SaveChangesAsync();

        // Sample Harvest Lot
        var paddyCrop = await _context.Crops.FirstOrDefaultAsync(c => c.NameBn.Contains("বোরো ধান"));
        if (paddyCrop != null)
        {
            var lot = new HarvestLot
            {
                LotNumber = "LOT-2026-BOG-0001",
                FarmerProfileId = farmerProfile.Id,
                CropId = paddyCrop.Id,
                Quantity = 50m, // 50 Mon = 2000 Kg
                Unit = WeightUnit.Mon,
                Grade = QualityGrade.GradeA,
                GradeDescription = "শুকনো ও পরিষ্কার সোনালী দানার বোরো ব্রি-২৮ ধান, আর্দ্রতা ১৪% এর নিচে।",
                AskingPricePerKg = 34.50m,
                HarvestDate = DateTime.UtcNow.AddDays(-5),
                AvailableFromDate = DateTime.UtcNow,
                PickupDistrict = "বগুড়া (Bogura)",
                PickupUpazila = "শিবগঞ্জ (Shibganj)",
                PickupLocationDetails = "মহাস্থান হাট সংলগ্ন কৃষক পয়েন্ট",
                Status = LotStatus.ActiveListed,
                Notes = "খামার গেট থেকে সরাসরি ডেলিভারি দেওয়া সম্ভব।"
            };
            await _context.HarvestLots.AddAsync(lot);
            await _context.SaveChangesAsync();

            // Sample Buyer Offer
            var offer = new BuyerOffer
            {
                OfferCode = "OFR-2026-0001",
                HarvestLotId = lot.Id,
                BuyerProfileId = buyerProfile.Id,
                OfferedPricePerKg = 35.00m,
                OfferedQuantityKg = 2000m,
                TransportCostDeduction = 1500.00m,
                WeighingAndLaborCostDeduction = 600.00m,
                PackagingCostDeduction = 800.00m,
                PlatformFee = 350.00m, // 0.5%
                OtherDeductions = 0,
                ExpiryDateUtc = DateTime.UtcNow.AddDays(4),
                DeliveryTerms = "খামার গেট থেকে ট্রাকযোগে সংগ্রহ (Farmgate Pickup)",
                PaymentTerms = "ওজন সম্পন্ন হওয়ার ১ ঘণ্টার মধ্যে ব্যাংক/বিকাশ ট্রান্সফার",
                Remarks = "আমরা পুরো ২,০০০ কেজিই একবারে ক্রয় করতে আগ্রহী।",
                Status = OfferStatus.Submitted
            };
            await _context.BuyerOffers.AddAsync(offer);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedMarketIntelligenceAsync()
    {
        if (await _context.Markets.AnyAsync()) return;

        // 1. Prominent Bangladeshi wholesale & retail markets
        var kawranBazar = new Market
        {
            MarketCode = "MKT-DHK-001",
            NameBn = "কারওয়ান বাজার পাইকারি ও খুচরা আড়ত",
            NameEn = "Kawran Bazar Wholesale & Retail Hub",
            MarketType = MarketType.Both,
            District = "ঢাকা (Dhaka)",
            Upazila = "তেজগাঁও (Tejgaon)",
            AddressDetails = "কারওয়ান বাজার মোড়, তেজগাঁও, ঢাকা",
            Latitude = 23.7516m,
            Longitude = 90.3944m,
            IsActive = true
        };

        var mahasthanHat = new Market
        {
            MarketCode = "MKT-BOG-002",
            NameBn = "মহাস্থান হাট (বৃহত্তম কৃষিপণ্য মোকাম)",
            NameEn = "Mahasthan Hat Agricultural Trading Center",
            MarketType = MarketType.Wholesale,
            District = "বগুড়া (Bogura)",
            Upazila = "শিবগঞ্জ (Shibganj)",
            AddressDetails = "ঢাকা-রংপুর মহাসড়ক সংলগ্ন, মহাস্থানগড়, শিবগঞ্জ",
            Latitude = 24.9622m,
            Longitude = 89.3456m,
            IsActive = true
        };

        var khatunganj = new Market
        {
            MarketCode = "MKT-CTG-003",
            NameBn = "খাতুনগঞ্জ পাইকারি মোকাম",
            NameEn = "Khatunganj Commodity Market",
            MarketType = MarketType.Wholesale,
            District = "চট্টগ্রাম (Chattogram)",
            Upazila = "কোতোয়ালী (Kotwali)",
            AddressDetails = "খাতুনগঞ্জ বাণিজ্যিক এলাকা, চাক্তাই, চট্টগ্রাম",
            Latitude = 22.3364m,
            Longitude = 91.8385m,
            IsActive = true
        };

        var badamtoli = new Market
        {
            MarketCode = "MKT-DHK-004",
            NameBn = "বাদামতলী ফল ও পাইকারি আড়ত",
            NameEn = "Badamtoli Wholesale Market",
            MarketType = MarketType.Wholesale,
            District = "ঢাকা (Dhaka)",
            Upazila = "কোতোয়ালী (Kotwali)",
            AddressDetails = "বুড়িগঙ্গা নদী তীরবর্তী বাদামতলী ঘাট",
            Latitude = 23.7104m,
            Longitude = 90.4074m,
            IsActive = true
        };

        var rajshahiBazar = new Market
        {
            MarketCode = "MKT-RAJ-005",
            NameBn = "রাজশাহী সাহেব বাজার কাঁচাবাজার",
            NameEn = "Rajshahi Shaheb Bazar Local Market",
            MarketType = MarketType.Retail,
            District = "রাজশাহী (Rajshahi)",
            Upazila = "বোয়ালিয়া (Boalia)",
            AddressDetails = "সাহেব বাজার জিরো পয়েন্ট, রাজশাহী",
            Latitude = 24.3636m,
            Longitude = 88.6009m,
            IsActive = true
        };

        var jashoreBazar = new Market
        {
            MarketCode = "MKT-JAS-006",
            NameBn = "যশোর বড় বাজার মোকাম",
            NameEn = "Jashore Boro Bazar Commercial Hub",
            MarketType = MarketType.Both,
            District = "যশোর (Jashore)",
            Upazila = "কোতোয়ালী (Kotwali)",
            AddressDetails = "বড় বাজার রোড, ভৈরব নদের তীরবর্তী",
            Latitude = 23.1678m,
            Longitude = 89.2089m,
            IsActive = true
        };

        await _context.Markets.AddRangeAsync(kawranBazar, mahasthanHat, khatunganj, badamtoli, rajshahiBazar, jashoreBazar);
        await _context.SaveChangesAsync();

        // 2. Verified Data Sources
        var damSource = new PriceSource
        {
            NameBn = "কৃষি বিপণন অধিদপ্তর (DAM)",
            NameEn = "Department of Agricultural Marketing",
            SourceType = PriceSourceType.GovernmentOfficial,
            OrganizationName = "কৃষি মন্ত্রণালয়, গণপ্রজাতন্ত্রী বাংলাদেশ সরকার",
            IsOfficialAuthority = true,
            Description = "দৈনিক কৃষি পণ্য বাজারদর মনিটরিং পোর্টাল ও জেলা বিপণন কর্মকর্তা প্রতিবেদন"
        };

        var dncrpSource = new PriceSource
        {
            NameBn = "জাতীয় ভোক্তা-অধিকার সংরক্ষণ অধিদপ্তর (DNCRP)",
            NameEn = "Directorate of National Consumer Rights Protection",
            SourceType = PriceSourceType.GovernmentOfficial,
            OrganizationName = "বাণিজ্য মন্ত্রণালয়",
            IsOfficialAuthority = true,
            Description = "ভোক্তা অধিকার সংরক্ষণ ও আইনি মূল্য নির্দেশিকা তদারকি শাখা"
        };

        var tcbSource = new PriceSource
        {
            NameBn = "ট্রেডিং কর্পোরেশন অব বাংলাদেশ (TCB)",
            NameEn = "Trading Corporation of Bangladesh",
            SourceType = PriceSourceType.GovernmentOfficial,
            OrganizationName = "বাণিজ্য মন্ত্রণালয়",
            IsOfficialAuthority = true,
            Description = "নিত্যপ্রয়োজনীয় পণ্যের বাজারদর পর্যবেক্ষণ ও ট্রাকসেল মনিটরিং"
        };

        var committeeSource = new PriceSource
        {
            NameBn = "মহাস্থান হাট আড়তদার ও ব্যবসায়ী সমিতি",
            NameEn = "Mahasthan Hat Merchant Committee",
            SourceType = PriceSourceType.MarketCommittee,
            OrganizationName = "মহাস্থান কাঁচামাল আড়ত সমিতি",
            IsOfficialAuthority = false,
            Description = "স্থানীয় আড়তদারদের প্রকৃত দৈনিক পাইকারি বেচাকেনার গড় দর"
        };

        var fieldSurveySource = new PriceSource
        {
            NameBn = "মাঠপর্যায়ের বাজার মনিটরিং প্রতিনিধি",
            NameEn = "Field Market Survey Agents",
            SourceType = PriceSourceType.MarketSurvey,
            OrganizationName = "কৃষিবন্ধু মূল্য নিরীক্ষা দল",
            IsOfficialAuthority = false,
            Description = "সরাসরি বাজারে উপস্থিত হয়ে সংগৃহীত পাইকারি ও খুচরা মূল্য"
        };

        await _context.PriceSources.AddRangeAsync(damSource, dncrpSource, tcbSource, committeeSource, fieldSurveySource);
        await _context.SaveChangesAsync();

        // 3. Official Price Directives (Statutory government price bounds)
        var potatoCrop = await _context.Crops.FirstOrDefaultAsync(c => c.NameBn.Contains("আলু"));
        var onionCrop = await _context.Crops.FirstOrDefaultAsync(c => c.NameBn.Contains("পেঁয়াজ"));

        if (potatoCrop != null)
        {
            var potatoDirective = new OfficialPriceDirective
            {
                DirectiveNumber = "DNCRP-2026-015",
                TitleBn = "আলুর সর্বোচ্চ খুচরা ও হিমাগার গেট যৌক্তিক মূল্য নির্ধারণ আদেশ",
                IssuingAuthority = "জাতীয় ভোক্তা-অধিকার সংরক্ষণ অধিদপ্তর (DNCRP)",
                SourceReference = "স্মারক নং: ৩৮.০১.০০০০.০২৭.১২.০৩৪.২৬, বাংলাদেশ গেজেট অতিরিক্ত সংখ্যা",
                CropId = potatoCrop.Id,
                MarketType = MarketType.Both,
                ApplicableDistrict = null, // Nationwide
                ApplicableUpazila = null,
                EffectiveFromUtc = DateTime.UtcNow.AddDays(-30),
                EffectiveToUtc = DateTime.UtcNow.AddDays(60),
                OfficialBenchmarkPrice = 22.00m,
                LegalMaxPrice = 28.00m,
                StandardUnit = "কেজি",
                ApplicableConditions = "সকল প্রকার সাদা গোল ও ডায়মন্ড আলুর ক্ষেত্রে পাইকারি সর্বোচ্চ ২৫ টাকা এবং খুচরা পর্যায়ে সর্বোচ্চ ২৮ টাকা/কেজি প্রযোজ্য।",
                VerificationStatus = PriceVerificationStatus.Verified,
                IsActive = true
            };
            await _context.OfficialPriceDirectives.AddAsync(potatoDirective);
        }

        if (onionCrop != null)
        {
            var onionDirective = new OfficialPriceDirective
            {
                DirectiveNumber = "DAM-2026-022",
                TitleBn = "দেশি তাহেরপুরী পেঁয়াজ যৌক্তিক বাজারদর সংক্রান্ত গণবিজ্ঞপ্তি",
                IssuingAuthority = "কৃষি বিপণন অধিদপ্তর (DAM)",
                SourceReference = "কৃবিঅ/মূল্য/২০২৬/১১৮",
                CropId = onionCrop.Id,
                MarketType = MarketType.Retail,
                ApplicableDistrict = null,
                ApplicableUpazila = null,
                EffectiveFromUtc = DateTime.UtcNow.AddDays(-15),
                EffectiveToUtc = DateTime.UtcNow.AddDays(45),
                OfficialBenchmarkPrice = 60.00m,
                LegalMaxPrice = 75.00m,
                StandardUnit = "কেজি",
                ApplicableConditions = "খোলা দেশি পেঁয়াজ খুচরা পর্যায়ে সর্বোচ্চ ৭৫ টাকা/কেজি।",
                VerificationStatus = PriceVerificationStatus.Verified,
                IsActive = true
            };
            await _context.OfficialPriceDirectives.AddAsync(onionDirective);
        }

        await _context.SaveChangesAsync();

        // 4. Real Stored Observations across the last 7 days (Used for Summary and Trend Charts)
        var today = DateTime.UtcNow.Date;
        var observations = new List<MarketPriceObservation>();

        if (potatoCrop != null)
        {
            // Potato observations across 7 days
            decimal[] mahasthanWholesalePrices = [22.0m, 22.5m, 23.0m, 23.0m, 23.5m, 24.0m, 24.5m];
            decimal[] kawranWholesalePrices = [24.0m, 24.5m, 24.5m, 25.0m, 25.0m, 25.5m, 26.0m];
            decimal[] kawranRetailPrices = [28.0m, 28.5m, 29.0m, 29.5m, 29.5m, 30.0m, 30.5m];

            for (int i = 0; i < 7; i++)
            {
                var obsDate = today.AddDays(-6 + i);

                // Mahasthan wholesale
                observations.Add(new MarketPriceObservation
                {
                    CropId = potatoCrop.Id,
                    MarketId = mahasthanHat.Id,
                    MarketType = MarketType.Wholesale,
                    ObservationDate = obsDate,
                    PriceSourceId = committeeSource.Id,
                    ObservedPrice = mahasthanWholesalePrices[i] * 40m, // recorded per Mon
                    ObservedQuantity = 1.0m,
                    ObservedUnit = "মণ",
                    NormalizedPricePerUnit = mahasthanWholesalePrices[i],
                    NormalizedUnit = "কেজি",
                    MinPrice = (mahasthanWholesalePrices[i] - 1.0m) * 40m,
                    MaxPrice = (mahasthanWholesalePrices[i] + 1.0m) * 40m,
                    NormalizedMinPrice = mahasthanWholesalePrices[i] - 1.0m,
                    NormalizedMaxPrice = mahasthanWholesalePrices[i] + 1.0m,
                    Grade = QualityGrade.GradeA,
                    VerificationStatus = PriceVerificationStatus.Verified,
                    Notes = "আলুর প্রধান মোকাম মহাস্থান হাট"
                });

                // Kawran wholesale
                observations.Add(new MarketPriceObservation
                {
                    CropId = potatoCrop.Id,
                    MarketId = kawranBazar.Id,
                    MarketType = MarketType.Wholesale,
                    ObservationDate = obsDate,
                    PriceSourceId = damSource.Id,
                    ObservedPrice = kawranWholesalePrices[i],
                    ObservedQuantity = 1.0m,
                    ObservedUnit = "কেজি",
                    NormalizedPricePerUnit = kawranWholesalePrices[i],
                    NormalizedUnit = "কেজি",
                    MinPrice = kawranWholesalePrices[i] - 0.5m,
                    MaxPrice = kawranWholesalePrices[i] + 0.5m,
                    NormalizedMinPrice = kawranWholesalePrices[i] - 0.5m,
                    NormalizedMaxPrice = kawranWholesalePrices[i] + 0.5m,
                    Grade = QualityGrade.Standard,
                    VerificationStatus = PriceVerificationStatus.Verified,
                    Notes = "কারওয়ান বাজার আড়ত পাইকারি দর"
                });

                // Kawran retail
                observations.Add(new MarketPriceObservation
                {
                    CropId = potatoCrop.Id,
                    MarketId = kawranBazar.Id,
                    MarketType = MarketType.Retail,
                    ObservationDate = obsDate,
                    PriceSourceId = fieldSurveySource.Id,
                    ObservedPrice = kawranRetailPrices[i],
                    ObservedQuantity = 1.0m,
                    ObservedUnit = "কেজি",
                    NormalizedPricePerUnit = kawranRetailPrices[i],
                    NormalizedUnit = "কেজি",
                    MinPrice = kawranRetailPrices[i] - 1.0m,
                    MaxPrice = kawranRetailPrices[i] + 1.0m,
                    NormalizedMinPrice = kawranRetailPrices[i] - 1.0m,
                    NormalizedMaxPrice = kawranRetailPrices[i] + 1.0m,
                    Grade = QualityGrade.Standard,
                    VerificationStatus = PriceVerificationStatus.Verified,
                    Notes = "খুচরা বাজার দর"
                });
            }
        }

        if (onionCrop != null)
        {
            // Onion observations across 7 days
            decimal[] onionWholesale = [62.0m, 63.0m, 63.5m, 64.0m, 65.0m, 65.5m, 66.0m];
            decimal[] onionRetail = [72.0m, 72.0m, 73.0m, 74.0m, 74.5m, 75.0m, 76.0m];

            for (int i = 0; i < 7; i++)
            {
                var obsDate = today.AddDays(-6 + i);

                observations.Add(new MarketPriceObservation
                {
                    CropId = onionCrop.Id,
                    MarketId = kawranBazar.Id,
                    MarketType = MarketType.Wholesale,
                    ObservationDate = obsDate,
                    PriceSourceId = damSource.Id,
                    ObservedPrice = onionWholesale[i],
                    ObservedQuantity = 1.0m,
                    ObservedUnit = "কেজি",
                    NormalizedPricePerUnit = onionWholesale[i],
                    NormalizedUnit = "কেজি",
                    MinPrice = onionWholesale[i] - 2m,
                    MaxPrice = onionWholesale[i] + 2m,
                    NormalizedMinPrice = onionWholesale[i] - 2m,
                    NormalizedMaxPrice = onionWholesale[i] + 2m,
                    Grade = QualityGrade.GradeA,
                    VerificationStatus = PriceVerificationStatus.Verified
                });

                observations.Add(new MarketPriceObservation
                {
                    CropId = onionCrop.Id,
                    MarketId = kawranBazar.Id,
                    MarketType = MarketType.Retail,
                    ObservationDate = obsDate,
                    PriceSourceId = fieldSurveySource.Id,
                    ObservedPrice = onionRetail[i],
                    ObservedQuantity = 1.0m,
                    ObservedUnit = "কেজি",
                    NormalizedPricePerUnit = onionRetail[i],
                    NormalizedUnit = "কেজি",
                    MinPrice = onionRetail[i] - 2m,
                    MaxPrice = onionRetail[i] + 2m,
                    NormalizedMinPrice = onionRetail[i] - 2m,
                    NormalizedMaxPrice = onionRetail[i] + 2m,
                    Grade = QualityGrade.Standard,
                    VerificationStatus = PriceVerificationStatus.Verified
                });
            }
        }

        await _context.MarketPriceObservations.AddRangeAsync(observations);
        await _context.SaveChangesAsync();

        // 5. Seed Initial Sample Consumer Complaints for Demonstration
        if (potatoCrop != null)
        {
            var sampleComplaint = new ConsumerComplaint
            {
                TrackingNumber = "CMP-2026-00001",
                ComplainantName = "মো. তানভীর আহমেদ",
                ComplainantPhone = "01722334455",
                ComplainantEmail = "tanvir.consumer@gmail.com",
                ShopOrBusinessName = "মেসার্স সততা জেনারেল স্টোর",
                ShopAddress = "দোকান নং ৪২, কাঁচাবাজার গলি, কারওয়ান বাজার",
                MarketId = kawranBazar.Id,
                District = "ঢাকা (Dhaka)",
                Upazila = "তেজগাঁও (Tejgaon)",
                SellerType = MarketType.Retail,
                CropId = potatoCrop.Id,
                QuantityPurchased = 5.0m,
                ReportedUnit = "কেজি",
                ActualPriceCharged = 175.0m, // 35 BDT/kg
                NormalizedPricePerUnit = 35.0m,
                StandardUnit = "কেজি",
                PurchaseDateTime = DateTime.UtcNow.AddDays(-1),
                Description = "সরকার নির্ধারিত সর্বোচ্চ খুচরা মূল্য ২৮ টাকা থাকা সত্ত্বেও দোকানদার প্রতি কেজি ৩৫ টাকা দাবি করে এবং পাকা রসিদ দিতে অস্বীকৃতি জানায়।",
                Status = ComplaintStatus.UnderReview,
                MatchedMarketAveragePrice = 30.0m,
                MatchedMarketMinPrice = 29.0m,
                MatchedMarketMaxPrice = 31.0m,
                PriceVariancePercentage = 16.67m,
                OfficialPriceDirectiveId = (await _context.OfficialPriceDirectives.FirstOrDefaultAsync(d => d.CropId == potatoCrop.Id))?.Id,
                OfficialLegalPrice = 28.0m,
                IsOfficialPriceExceeded = true,
                ComparisonAnalysisNotes = "অভিযুক্ত বিক্রয়মূল্য: ৩৫.০০ টাকা/কেজি। একই তারিখে সংশ্লিষ্ট এলাকায় পর্যবেক্ষণকৃত গড় বাজারদর ছিল ৩০.০০ টাকা/কেজি। আইনগত সরকারি মূল্য নির্দেশনা (DNCRP-2026-015) অনুসারে সর্বোচ্চ নির্ধারিত মূল্য ২৮.০০ টাকা/কেজি। দাখিলকৃত মূল্য সরকারি সর্বোচ্চ আইনি নির্দেশনার চেয়ে বেশি দেখা যাচ্ছে; এটি ভোক্তা অধিকার বিধি অনুযায়ী আনুষ্ঠানিক পর্যালোচনার যোগ্য।",
                IsPubliclyVisible = false,
                CreatedBy = "PublicUser"
            };

            await _context.ConsumerComplaints.AddAsync(sampleComplaint);
            await _context.SaveChangesAsync();
        }
    }
}
