using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using KrishiBondhu.Application.Common.Interfaces;
using KrishiBondhu.Application.Interfaces.Repositories;
using KrishiBondhu.Application.Interfaces.Services;
using KrishiBondhu.Domain.Entities;
using KrishiBondhu.Infrastructure.Data;
using KrishiBondhu.Infrastructure.Identity;
using KrishiBondhu.Infrastructure.Repositories;
using KrishiBondhu.Infrastructure.Services;

namespace KrishiBondhu.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var provider = configuration["DatabaseProvider"] ?? "PostgreSQL";
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
                               ?? "Server=172.16.102.123;Port=5432;Database=EPrintDb;User Id=postgres;Password=abcd123!";

        services.AddDbContext<KrishiBondhuDbContext>(options =>
        {
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

            if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(KrishiBondhuDbContext).Assembly.FullName);
                    npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
                });
            }
        });

        // ASP.NET Core Identity
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = false;
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<KrishiBondhuDbContext>()
        .AddDefaultTokenProviders();

        // Cookie Authentication Configuration
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = ".KrishiBondhu.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/access-denied";
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
        });

        services.AddHttpContextAccessor();

        // Repositories & Unit of Work
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IHarvestLotRepository, HarvestLotRepository>();
        services.AddScoped<IBuyerOfferRepository, BuyerOfferRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IWeighingRecordRepository, WeighingRecordRepository>();
        services.AddScoped<IPaymentRecordRepository, PaymentRecordRepository>();
        services.AddScoped<IDisputeCaseRepository, DisputeCaseRepository>();
        services.AddScoped<ICropRepository, CropRepository>();
        services.AddScoped<IFarmerProfileRepository, FarmerProfileRepository>();
        services.AddScoped<IBuyerProfileRepository, BuyerProfileRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IMarketRepository, MarketRepository>();
        services.AddScoped<IPriceSourceRepository, PriceSourceRepository>();
        services.AddScoped<IMarketPriceObservationRepository, MarketPriceObservationRepository>();
        services.AddScoped<IOfficialPriceDirectiveRepository, OfficialPriceDirectiveRepository>();
        services.AddScoped<IConsumerComplaintRepository, ConsumerComplaintRepository>();

        // Core Infrastructure Services
        services.AddSingleton<ISignInTokenStore, InMemorySignInTokenStore>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddSingleton<ILocalizationService, LocalizationService>();

        // Application Business Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IFarmerService, FarmerService>();
        services.AddScoped<IBuyerService, BuyerService>();
        services.AddScoped<ICropService, CropService>();
        services.AddScoped<IHarvestLotService, HarvestLotService>();
        services.AddScoped<IOfferComparisonService, OfferComparisonService>();
        services.AddScoped<IOrderLifecycleService, OrderLifecycleService>();
        services.AddScoped<IWeighingService, WeighingService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IDisputeService, DisputeService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IMarketPriceService, MarketPriceService>();
        services.AddScoped<IConsumerComplaintService, ConsumerComplaintService>();

        // Seeder
        services.AddScoped<DbInitializer>();

        return services;
    }
}
