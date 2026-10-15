using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Username { get; }
    string? FullName { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(UserRole role);
}

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

public interface IFileStorageService
{
    Task<(string storedFileName, string relativePath)> SaveFileAsync(Stream fileStream, string originalFileName, string contentType, string category, CancellationToken cancellationToken = default);
    Task<Stream?> GetFileStreamAsync(string relativePath, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default);
    (bool isValid, string? errorMessage) ValidateFile(string fileName, long fileSizeBytes, string contentType);
}

public interface IAuditLogger
{
    Task LogAsync(string entityName, string entityId, string action, string? detailsJson = null, CancellationToken cancellationToken = default);
}
