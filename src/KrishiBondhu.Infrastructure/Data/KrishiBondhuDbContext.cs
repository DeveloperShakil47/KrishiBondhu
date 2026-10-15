using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using KrishiBondhu.Domain.Common;
using KrishiBondhu.Domain.Entities;
using KrishiBondhu.Infrastructure.Identity;

namespace KrishiBondhu.Infrastructure.Data;

public class KrishiBondhuDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public KrishiBondhuDbContext(DbContextOptions<KrishiBondhuDbContext> options) : base(options)
    {
    }

    public DbSet<FarmerProfile> FarmerProfiles => Set<FarmerProfile>();
    public DbSet<FarmPlot> FarmPlots => Set<FarmPlot>();
    public DbSet<BuyerProfile> BuyerProfiles => Set<BuyerProfile>();
    public DbSet<Crop> Crops => Set<Crop>();
    public DbSet<HarvestLot> HarvestLots => Set<HarvestLot>();
    public DbSet<BuyerDemand> BuyerDemands => Set<BuyerDemand>();
    public DbSet<BuyerOffer> BuyerOffers => Set<BuyerOffer>();
    public DbSet<OfferCost> OfferCosts => Set<OfferCost>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<WeighingRecord> WeighingRecords => Set<WeighingRecord>();
    public DbSet<PaymentRecord> PaymentRecords => Set<PaymentRecord>();
    public DbSet<DeliveryRecord> DeliveryRecords => Set<DeliveryRecord>();
    public DbSet<EvidenceRecord> EvidenceRecords => Set<EvidenceRecord>();
    public DbSet<DisputeCase> DisputeCases => Set<DisputeCase>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // Daily Market Prices & Directives
    public DbSet<Market> Markets => Set<Market>();
    public DbSet<PriceSource> PriceSources => Set<PriceSource>();
    public DbSet<MarketPriceReport> MarketPriceReports => Set<MarketPriceReport>();
    public DbSet<MarketPriceObservation> MarketPriceObservations => Set<MarketPriceObservation>();
    public DbSet<OfficialPriceDirective> OfficialPriceDirectives => Set<OfficialPriceDirective>();

    // Consumer Complaint Management
    public DbSet<ConsumerComplaint> ConsumerComplaints => Set<ConsumerComplaint>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Customize Identity table names
        builder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("Users");
            b.Property(u => u.FullName).HasMaxLength(150).IsRequired();
            b.Property(u => u.PhoneNumber).HasMaxLength(20).IsRequired();
            b.Property(u => u.District).HasMaxLength(50).IsRequired();
            b.Property(u => u.Upazila).HasMaxLength(50).IsRequired();
        });

        builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        // FarmerProfile
        builder.Entity<FarmerProfile>(b =>
        {
            b.ToTable("FarmerProfiles");
            b.HasIndex(f => f.UserId).IsUnique();
            b.Property(f => f.FullNameBn).HasMaxLength(150).IsRequired();
            b.Property(f => f.PhoneNumber).HasMaxLength(20).IsRequired();
            b.Property(f => f.TotalLandAreaAcres).HasPrecision(18, 2);
            b.HasMany(f => f.Plots).WithOne(p => p.FarmerProfile).HasForeignKey(p => p.FarmerProfileId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(f => f.HarvestLots).WithOne(h => h.FarmerProfile).HasForeignKey(h => h.FarmerProfileId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(f => f.Orders).WithOne(o => o.FarmerProfile).HasForeignKey(o => o.FarmerProfileId).OnDelete(DeleteBehavior.Restrict);
        });

        // FarmPlot
        builder.Entity<FarmPlot>(b =>
        {
            b.ToTable("FarmPlots");
            b.Property(p => p.AreaAcres).HasPrecision(18, 2);
        });

        // BuyerProfile
        builder.Entity<BuyerProfile>(b =>
        {
            b.ToTable("BuyerProfiles");
            b.HasIndex(bp => bp.UserId).IsUnique();
            b.Property(bp => bp.BusinessName).HasMaxLength(200).IsRequired();
            b.Property(bp => bp.PhoneNumber).HasMaxLength(20).IsRequired();
            b.Property(bp => bp.Rating).HasPrecision(5, 2);
            b.HasMany(bp => bp.Demands).WithOne(d => d.BuyerProfile).HasForeignKey(d => d.BuyerProfileId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(bp => bp.Offers).WithOne(o => o.BuyerProfile).HasForeignKey(o => o.BuyerProfileId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(bp => bp.Orders).WithOne(o => o.BuyerProfile).HasForeignKey(o => o.BuyerProfileId).OnDelete(DeleteBehavior.Restrict);
        });

        // Crop
        builder.Entity<Crop>(b =>
        {
            b.ToTable("Crops");
            b.Property(c => c.NameBn).HasMaxLength(100).IsRequired();
            b.Property(c => c.NameEn).HasMaxLength(100).IsRequired();
            b.Property(c => c.TypicalMinPricePerKg).HasPrecision(18, 2);
            b.Property(c => c.TypicalMaxPricePerKg).HasPrecision(18, 2);
        });

        // HarvestLot
        builder.Entity<HarvestLot>(b =>
        {
            b.ToTable("HarvestLots");
            b.HasIndex(h => h.LotNumber).IsUnique();
            b.Property(h => h.LotNumber).HasMaxLength(50).IsRequired();
            b.Property(h => h.Quantity).HasPrecision(18, 2);
            b.Property(h => h.AskingPricePerKg).HasPrecision(18, 2);
            b.HasOne(h => h.Crop).WithMany(c => c.HarvestLots).HasForeignKey(h => h.CropId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(h => h.Offers).WithOne(o => o.HarvestLot).HasForeignKey(o => o.HarvestLotId).OnDelete(DeleteBehavior.Restrict);
        });

        // BuyerDemand
        builder.Entity<BuyerDemand>(b =>
        {
            b.ToTable("BuyerDemands");
            b.Property(d => d.RequiredQuantityKg).HasPrecision(18, 2);
            b.Property(d => d.TargetPricePerKg).HasPrecision(18, 2);
            b.HasOne(d => d.Crop).WithMany(c => c.BuyerDemands).HasForeignKey(d => d.CropId).OnDelete(DeleteBehavior.Restrict);
        });

        // BuyerOffer
        builder.Entity<BuyerOffer>(b =>
        {
            b.ToTable("BuyerOffers");
            b.HasIndex(o => o.OfferCode).IsUnique();
            b.Property(o => o.OfferCode).HasMaxLength(50).IsRequired();
            b.Property(o => o.OfferedPricePerKg).HasPrecision(18, 2);
            b.Property(o => o.OfferedQuantityKg).HasPrecision(18, 2);
            b.Property(o => o.TransportCostDeduction).HasPrecision(18, 2);
            b.Property(o => o.WeighingAndLaborCostDeduction).HasPrecision(18, 2);
            b.Property(o => o.PackagingCostDeduction).HasPrecision(18, 2);
            b.Property(o => o.PlatformFee).HasPrecision(18, 2);
            b.Property(o => o.OtherDeductions).HasPrecision(18, 2);

            b.HasMany(o => o.ItemizedCosts).WithOne(c => c.BuyerOffer).HasForeignKey(c => c.BuyerOfferId).OnDelete(DeleteBehavior.Cascade);
        });

        // OfferCost
        builder.Entity<OfferCost>(b =>
        {
            b.ToTable("OfferCosts");
            b.Property(c => c.Amount).HasPrecision(18, 2);
        });

        // Order
        builder.Entity<Order>(b =>
        {
            b.ToTable("Orders");
            b.HasIndex(o => o.OrderNumber).IsUnique();
            b.HasIndex(o => o.IdempotencyKey).IsUnique();
            b.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
            b.Property(o => o.IdempotencyKey).HasMaxLength(100).IsRequired();

            b.Property(o => o.AgreedPricePerKg).HasPrecision(18, 2);
            b.Property(o => o.AgreedQuantityKg).HasPrecision(18, 2);
            b.Property(o => o.AgreedTransportDeduction).HasPrecision(18, 2);
            b.Property(o => o.AgreedWeighingDeduction).HasPrecision(18, 2);
            b.Property(o => o.AgreedPackagingDeduction).HasPrecision(18, 2);
            b.Property(o => o.AgreedPlatformFee).HasPrecision(18, 2);
            b.Property(o => o.AgreedOtherDeductions).HasPrecision(18, 2);

            b.Property(o => o.VerifiedNetQuantityKg).HasPrecision(18, 2);
            b.Property(o => o.VerifiedPaidAmount).HasPrecision(18, 2);

            b.HasOne(o => o.HarvestLot).WithMany(h => h.Orders).HasForeignKey(o => o.HarvestLotId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(o => o.BuyerOffer).WithOne(b => b.ResultingOrder).HasForeignKey<Order>(o => o.BuyerOfferId).OnDelete(DeleteBehavior.Restrict);

            b.HasMany(o => o.WeighingRecords).WithOne(w => w.Order).HasForeignKey(w => w.OrderId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(o => o.PaymentRecords).WithOne(p => p.Order).HasForeignKey(p => p.OrderId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(o => o.DeliveryRecords).WithOne(d => d.Order).HasForeignKey(d => d.OrderId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(o => o.Disputes).WithOne(d => d.Order).HasForeignKey(d => d.OrderId).OnDelete(DeleteBehavior.Restrict);
        });

        // WeighingRecord
        builder.Entity<WeighingRecord>(b =>
        {
            b.ToTable("WeighingRecords");
            b.Property(w => w.GrossWeight).HasPrecision(18, 2);
            b.Property(w => w.TareWeight).HasPrecision(18, 2);
            b.Property(w => w.NetWeight).HasPrecision(18, 2);
        });

        // PaymentRecord
        builder.Entity<PaymentRecord>(b =>
        {
            b.ToTable("PaymentRecords");
            b.Property(p => p.Amount).HasPrecision(18, 2);
            b.Property(p => p.TransactionReference).HasMaxLength(100).IsRequired();
        });

        // DeliveryRecord
        builder.Entity<DeliveryRecord>(b =>
        {
            b.ToTable("DeliveryRecords");
            b.Property(d => d.ConsignmentNumber).HasMaxLength(100);
            b.Property(d => d.VehicleNumber).HasMaxLength(50);
        });

        // EvidenceRecord
        builder.Entity<EvidenceRecord>(b =>
        {
            b.ToTable("EvidenceRecords");
            b.Property(e => e.OriginalFileName).HasMaxLength(255).IsRequired();
            b.Property(e => e.StoredFileName).HasMaxLength(255).IsRequired();
        });

        // DisputeCase
        builder.Entity<DisputeCase>(b =>
        {
            b.ToTable("DisputeCases");
            b.HasIndex(d => d.DisputeNumber).IsUnique();
            b.Property(d => d.DisputeNumber).HasMaxLength(50).IsRequired();
            b.Property(d => d.DisputedAmount).HasPrecision(18, 2);
            b.Property(d => d.DisputedQuantityKg).HasPrecision(18, 2);
            b.Property(d => d.FinalAdjustedAmount).HasPrecision(18, 2);
        });

        // EvidenceRecord
        builder.Entity<EvidenceRecord>(b =>
        {
            b.ToTable("EvidenceRecords");
            b.Property(e => e.OriginalFileName).HasMaxLength(255).IsRequired();
            b.Property(e => e.StoredFileName).HasMaxLength(255).IsRequired();
        });

        // AuditLog
        builder.Entity<AuditLog>(b =>
        {
            b.ToTable("AuditLogs");
            b.Property(a => a.EntityName).HasMaxLength(100).IsRequired();
            b.Property(a => a.Action).HasMaxLength(50).IsRequired();
        });

        // Notification
        builder.Entity<Notification>(b =>
        {
            b.ToTable("Notifications");
            b.Property(n => n.TitleBn).HasMaxLength(200).IsRequired();
        });

        // Market
        builder.Entity<Market>(b =>
        {
            b.ToTable("Markets");
            b.HasIndex(m => m.MarketCode).IsUnique();
            b.Property(m => m.MarketCode).HasMaxLength(50).IsRequired();
            b.Property(m => m.NameBn).HasMaxLength(150).IsRequired();
            b.Property(m => m.NameEn).HasMaxLength(150).IsRequired();
            b.Property(m => m.District).HasMaxLength(50).IsRequired();
            b.Property(m => m.Upazila).HasMaxLength(50).IsRequired();
            b.Property(m => m.Latitude).HasPrecision(10, 7);
            b.Property(m => m.Longitude).HasPrecision(10, 7);
        });

        // PriceSource
        builder.Entity<PriceSource>(b =>
        {
            b.ToTable("PriceSources");
            b.Property(ps => ps.NameBn).HasMaxLength(150).IsRequired();
            b.Property(ps => ps.NameEn).HasMaxLength(150).IsRequired();
            b.Property(ps => ps.OrganizationName).HasMaxLength(200).IsRequired();
        });

        // MarketPriceReport
        builder.Entity<MarketPriceReport>(b =>
        {
            b.ToTable("MarketPriceReports");
            b.HasOne(r => r.Market).WithMany().HasForeignKey(r => r.MarketId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(r => r.PriceSource).WithMany(ps => ps.Reports).HasForeignKey(r => r.PriceSourceId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(r => r.Observations).WithOne(o => o.MarketPriceReport).HasForeignKey(o => o.MarketPriceReportId).OnDelete(DeleteBehavior.Cascade);
        });

        // MarketPriceObservation
        builder.Entity<MarketPriceObservation>(b =>
        {
            b.ToTable("MarketPriceObservations");
            b.Property(o => o.ObservedPrice).HasPrecision(18, 2);
            b.Property(o => o.ObservedQuantity).HasPrecision(18, 4);
            b.Property(o => o.NormalizedPricePerUnit).HasPrecision(18, 2);
            b.Property(o => o.MinPrice).HasPrecision(18, 2);
            b.Property(o => o.MaxPrice).HasPrecision(18, 2);
            b.Property(o => o.NormalizedMinPrice).HasPrecision(18, 2);
            b.Property(o => o.NormalizedMaxPrice).HasPrecision(18, 2);
            b.Property(o => o.ObservedUnit).HasMaxLength(50).IsRequired();
            b.Property(o => o.NormalizedUnit).HasMaxLength(50).IsRequired();
            b.Property(o => o.Currency).HasMaxLength(10).IsRequired();

            b.HasIndex(o => new { o.CropId, o.ObservationDate, o.MarketType, o.VerificationStatus });
            b.HasIndex(o => new { o.MarketId, o.ObservationDate });

            b.HasOne(o => o.Crop).WithMany().HasForeignKey(o => o.CropId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(o => o.Market).WithMany(m => m.Observations).HasForeignKey(o => o.MarketId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(o => o.PriceSource).WithMany(ps => ps.Observations).HasForeignKey(o => o.PriceSourceId).OnDelete(DeleteBehavior.Restrict);
        });

        // OfficialPriceDirective
        builder.Entity<OfficialPriceDirective>(b =>
        {
            b.ToTable("OfficialPriceDirectives");
            b.HasIndex(d => d.DirectiveNumber).IsUnique();
            b.Property(d => d.DirectiveNumber).HasMaxLength(100).IsRequired();
            b.Property(d => d.TitleBn).HasMaxLength(250).IsRequired();
            b.Property(d => d.IssuingAuthority).HasMaxLength(200).IsRequired();
            b.Property(d => d.SourceReference).HasMaxLength(250).IsRequired();
            b.Property(d => d.OfficialBenchmarkPrice).HasPrecision(18, 2);
            b.Property(d => d.LegalMaxPrice).HasPrecision(18, 2);
            b.Property(d => d.StandardUnit).HasMaxLength(50).IsRequired();

            b.HasIndex(d => new { d.CropId, d.EffectiveFromUtc, d.EffectiveToUtc, d.IsActive });

            b.HasOne(d => d.Crop).WithMany().HasForeignKey(d => d.CropId).OnDelete(DeleteBehavior.Restrict);
        });

        // ConsumerComplaint
        builder.Entity<ConsumerComplaint>(b =>
        {
            b.ToTable("ConsumerComplaints");
            b.HasIndex(c => c.TrackingNumber).IsUnique();
            b.HasIndex(c => c.ComplainantPhone);
            b.HasIndex(c => c.ClientIpHash);
            b.HasIndex(c => c.Status);
            b.HasIndex(c => c.CreatedAtUtc);

            b.Property(c => c.TrackingNumber).HasMaxLength(50).IsRequired();
            b.Property(c => c.ComplainantName).HasMaxLength(150).IsRequired();
            b.Property(c => c.ComplainantPhone).HasMaxLength(20).IsRequired();
            b.Property(c => c.ShopOrBusinessName).HasMaxLength(200).IsRequired();
            b.Property(c => c.ShopAddress).HasMaxLength(300).IsRequired();
            b.Property(c => c.District).HasMaxLength(50).IsRequired();
            b.Property(c => c.Upazila).HasMaxLength(50).IsRequired();

            b.Property(c => c.QuantityPurchased).HasPrecision(18, 4);
            b.Property(c => c.ActualPriceCharged).HasPrecision(18, 2);
            b.Property(c => c.NormalizedPricePerUnit).HasPrecision(18, 2);
            b.Property(c => c.ReportedUnit).HasMaxLength(50).IsRequired();
            b.Property(c => c.StandardUnit).HasMaxLength(50).IsRequired();

            b.Property(c => c.MatchedMarketAveragePrice).HasPrecision(18, 2);
            b.Property(c => c.MatchedMarketMinPrice).HasPrecision(18, 2);
            b.Property(c => c.MatchedMarketMaxPrice).HasPrecision(18, 2);
            b.Property(c => c.PriceVariancePercentage).HasPrecision(18, 2);
            b.Property(c => c.OfficialLegalPrice).HasPrecision(18, 2);

            b.HasOne(c => c.Crop).WithMany().HasForeignKey(c => c.CropId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(c => c.Market).WithMany(m => m.Complaints).HasForeignKey(c => c.MarketId).OnDelete(DeleteBehavior.SetNull);
            b.HasOne(c => c.OfficialPriceDirective).WithMany().HasForeignKey(c => c.OfficialPriceDirectiveId).OnDelete(DeleteBehavior.SetNull);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
