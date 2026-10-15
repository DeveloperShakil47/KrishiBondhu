using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using KrishiBondhu.Domain.Entities;
using KrishiBondhu.Domain.Enums;
using KrishiBondhu.Infrastructure.Data;
using KrishiBondhu.Infrastructure.Identity;

namespace KrishiBondhu.IntegrationTests;

public class TestDatabaseFixture : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    public KrishiBondhuDbContext Context { get; }

    public ApplicationUser SeedFarmerUser { get; private set; } = null!;
    public ApplicationUser SeedBuyerUser { get; private set; } = null!;
    public FarmerProfile SeedFarmer { get; private set; } = null!;
    public BuyerProfile SeedBuyer { get; private set; } = null!;
    public Crop SeedCrop { get; private set; } = null!;
    public HarvestLot SeedLot { get; private set; } = null!;
    public BuyerOffer SeedOffer { get; private set; } = null!;

    public TestDatabaseFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<KrishiBondhuDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new KrishiBondhuDbContext(options);
        Context.Database.EnsureCreated();

        SeedTestData();
    }

    private void SeedTestData()
    {
        SeedFarmerUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "01711000001",
            PhoneNumber = "01711000001",
            FullName = "মো. আবুল হোসেন",
            Role = UserRole.Farmer,
            District = "বগুড়া",
            Upazila = "শিবগঞ্জ"
        };
        Context.Users.Add(SeedFarmerUser);

        SeedBuyerUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "01811000002",
            PhoneNumber = "01811000002",
            FullName = "হাজী দেলোয়ার জাহান",
            Role = UserRole.Buyer,
            District = "বগুড়া",
            Upazila = "বগুড়া সদর"
        };
        Context.Users.Add(SeedBuyerUser);

        SeedCrop = new Crop
        {
            NameBn = "বোরো ধান (ব্রি ধান-২৮)",
            NameEn = "Boro Paddy (BRRI Dhan 28)",
            Category = "খাদ্যশস্য",
            TypicalMinPricePerKg = 32m,
            TypicalMaxPricePerKg = 36m,
            IsActive = true
        };
        Context.Crops.Add(SeedCrop);

        SeedFarmer = new FarmerProfile
        {
            UserId = SeedFarmerUser.Id,
            FullNameBn = "মো. আবুল হোসেন",
            FullNameEn = "Md. Abul Hossain",
            PhoneNumber = "01711000001",
            District = "বগুড়া",
            Upazila = "শিবগঞ্জ",
            IsActive = true
        };
        Context.FarmerProfiles.Add(SeedFarmer);

        SeedBuyer = new BuyerProfile
        {
            UserId = SeedBuyerUser.Id,
            BusinessName = "মেসার্স উত্তরা অ্যাগ্রো ট্রেডার্স",
            ContactPersonName = "হাজী দেলোয়ার",
            PhoneNumber = "01811000002",
            BusinessType = BuyerBusinessType.WholesalerAratdar,
            District = "বগুড়া",
            Upazila = "বগুড়া সদর",
            IsActive = true
        };
        Context.BuyerProfiles.Add(SeedBuyer);

        SeedLot = new HarvestLot
        {
            LotNumber = "LOT-2026-FIXTURE-001",
            FarmerProfileId = SeedFarmer.Id,
            CropId = SeedCrop.Id,
            Quantity = 50,
            Unit = WeightUnit.Mon,
            AskingPricePerKg = 35m,
            PickupDistrict = "বগুড়া",
            PickupUpazila = "শিবগঞ্জ",
            Status = LotStatus.ActiveListed
        };
        Context.HarvestLots.Add(SeedLot);

        SeedOffer = new BuyerOffer
        {
            OfferCode = "OFR-2026-FIXTURE-001",
            HarvestLotId = SeedLot.Id,
            BuyerProfileId = SeedBuyer.Id,
            OfferedPricePerKg = 35m,
            OfferedQuantityKg = 2000m,
            ExpiryDateUtc = DateTime.UtcNow.AddDays(7),
            Status = OfferStatus.Submitted
        };
        Context.BuyerOffers.Add(SeedOffer);

        Context.SaveChanges();
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
