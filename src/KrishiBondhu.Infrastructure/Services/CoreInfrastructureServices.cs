using System;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using KrishiBondhu.Application.Common.Interfaces;
using KrishiBondhu.Domain.Entities;
using KrishiBondhu.Domain.Enums;
using KrishiBondhu.Infrastructure.Data;

namespace KrishiBondhu.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(idClaim, out var guid) ? guid : null;
        }
    }

    public string? Username => User?.Identity?.Name;

    public string? FullName => User?.FindFirst("FullName")?.Value ?? Username;

    public UserRole? Role
    {
        get
        {
            var roleClaim = User?.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : null;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(UserRole role)
    {
        return User?.IsInRole(role.ToString()) ?? false;
    }
}

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storageBasePath;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf", ".webp" };
    private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp", "application/pdf" };
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public LocalFileStorageService(IConfiguration? configuration = null)
    {
        var relativeConfigPath = configuration?["PlatformSettings:StoragePath"] ?? "App_Data/EvidenceUploads";
        _storageBasePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relativeConfigPath));

        if (!Directory.Exists(_storageBasePath))
        {
            Directory.CreateDirectory(_storageBasePath);
        }
    }

    public (bool isValid, string? errorMessage) ValidateFile(string fileName, long fileSizeBytes, string contentType)
    {
        if (fileSizeBytes <= 0 || fileSizeBytes > MaxFileSizeBytes)
        {
            return (false, "ফাইলের সাইজ সর্বোচ্চ ১০ মেগাবাইট হতে হবে (Max 10 MB).");
        }

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
        {
            return (false, "শুধুমাত্র JPG, PNG, WEBP অথবা PDF ফাইল আপলোড করা যাবে।");
        }

        if (!AllowedContentTypes.Contains(contentType.ToLowerInvariant()))
        {
            return (false, "অবৈধ ফাইল কন্টেন্ট টাইপ।");
        }

        return (true, null);
    }

    public async Task<(string storedFileName, string relativePath)> SaveFileAsync(
        Stream fileStream,
        string originalFileName,
        string contentType,
        string category,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateFile(originalFileName, fileStream.Length, contentType);
        if (!validation.isValid)
        {
            throw new InvalidOperationException(validation.errorMessage);
        }

        var safeExt = Path.GetExtension(originalFileName).ToLowerInvariant();
        var storedFileName = $"{category}_{Guid.NewGuid():N}{safeExt}";
        var categoryDir = Path.Combine(_storageBasePath, category);

        if (!Directory.Exists(categoryDir))
        {
            Directory.CreateDirectory(categoryDir);
        }

        var fullFilePath = Path.Combine(categoryDir, storedFileName);
        using (var outputStream = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await fileStream.CopyToAsync(outputStream, cancellationToken);
        }

        var relativePath = Path.Combine(category, storedFileName).Replace('\\', '/');
        return (storedFileName, relativePath);
    }

    public Task<Stream?> GetFileStreamAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var safeRelative = relativePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_storageBasePath, safeRelative));

        // Prevent directory traversal attacks
        if (!fullPath.StartsWith(_storageBasePath, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("অবৈধ ফাইল পাথ (Access Denied).");
        }

        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(stream);
    }

    public Task<bool> DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var safeRelative = relativePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_storageBasePath, safeRelative));

        if (!fullPath.StartsWith(_storageBasePath, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(false);
        }

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}

public class AuditLogger : IAuditLogger
{
    private readonly KrishiBondhuDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogger(
        KrishiBondhuDbContext context,
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _currentUserService = currentUserService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string entityName, string entityId, string action, string? detailsJson = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            var auditLog = new AuditLog
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                PerformedByUserId = _currentUserService.UserId,
                PerformedByUsername = _currentUserService.Username,
                PerformedByRole = _currentUserService.Role?.ToString(),
                DetailsJson = detailsJson,
                IpAddress = ip,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Logging failure should not crash primary transaction
        }
    }
}
