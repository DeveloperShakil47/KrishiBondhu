using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using KrishiBondhu.Application.Common.Interfaces;
using KrishiBondhu.Infrastructure;
using KrishiBondhu.Infrastructure.Data;
using KrishiBondhu.Infrastructure.Identity;
using KrishiBondhu.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add Infrastructure & Application Services
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Auto-migrate and seed initial database data
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Initializing database and verifying schemas...");
        await initializer.InitializeAsync();
        logger.LogInformation("Database initialized and seed data verified successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialization encounter an issue. Application will continue with available services.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Clean HTTP Sign-in endpoint to write authentication cookies safely
app.MapGet("/api/auth/sign-in", async (string token, string? returnUrl, ISignInTokenStore tokenStore, HttpContext context) =>
{
    var entry = tokenStore.ConsumeToken(token);
    if (entry == null)
    {
        return Results.Redirect("/login?error=" + Uri.EscapeDataString("নিরাপত্তা টোকেনের মেয়াদ উত্তীর্ণ হয়েছে। অনুগ্রহ করে আবার লগইন করুন।"));
    }

    await context.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        entry.Value.Principal,
        entry.Value.Properties);

    var targetUrl = !string.IsNullOrWhiteSpace(returnUrl) && returnUrl.StartsWith('/') && !returnUrl.StartsWith("//")
        ? returnUrl
        : "/dashboard";

    return Results.LocalRedirect(targetUrl);
});

// Clean HTTP Sign-out endpoint
app.MapGet("/api/auth/sign-out", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

// Secure file viewing endpoint for receipts and evidence
app.MapGet("/api/evidence/{category}/{fileName}", async (string category, string fileName, IFileStorageService fileStorage, HttpContext context) =>
{
    if (!context.User.Identity?.IsAuthenticated ?? true)
    {
        return Results.Unauthorized();
    }

    var relativePath = $"{category}/{fileName}";
    var stream = await fileStorage.GetFileStreamAsync(relativePath);
    if (stream == null)
    {
        return Results.NotFound();
    }

    var ext = Path.GetExtension(fileName).ToLowerInvariant();
    var contentType = ext switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".pdf" => "application/pdf",
        _ => "application/octet-stream"
    };

    return Results.File(stream, contentType);
}).RequireAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
