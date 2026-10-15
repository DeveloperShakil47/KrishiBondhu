using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace KrishiBondhu.Infrastructure.Data;

public class KrishiBondhuDbContextFactory : IDesignTimeDbContextFactory<KrishiBondhuDbContext>
{
    public KrishiBondhuDbContext CreateDbContext(string[] args)
    {
        // Build configuration from Web project appsettings.json if present, or fallback
        var basePath = Directory.GetCurrentDirectory();
        var webPath = Path.Combine(basePath, "..", "KrishiBondhu.Web");
        if (!Directory.Exists(webPath))
        {
            webPath = Path.Combine(basePath, "src", "KrishiBondhu.Web");
        }

        var builder = new ConfigurationBuilder();
        if (Directory.Exists(webPath))
        {
            builder.SetBasePath(webPath);
        }
        else
        {
            builder.SetBasePath(basePath);
        }

        builder.AddJsonFile("appsettings.json", optional: true)
               .AddJsonFile("appsettings.Development.json", optional: true)
               .AddEnvironmentVariables();

        var configuration = builder.Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection") 
                               ?? "Server=172.16.102.123;Port=5432;Database=KrishiBondhuDb;User Id=postgres;Password=abcd123!";

        var optionsBuilder = new DbContextOptionsBuilder<KrishiBondhuDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(KrishiBondhuDbContext).Assembly.FullName);
        });

        return new KrishiBondhuDbContext(optionsBuilder.Options);
    }
}
