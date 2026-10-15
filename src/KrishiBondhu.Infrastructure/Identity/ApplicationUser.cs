using System;
using Microsoft.AspNetCore.Identity;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public string? NIdNumber { get; set; }
    public UserRole Role { get; set; } = UserRole.Farmer;
    public bool IsVerified { get; set; }
    public string District { get; set; } = string.Empty;
    public string Upazila { get; set; } = string.Empty;
    public string? Union { get; set; }
    public string PreferredLanguage { get; set; } = "bn";
    public string? ProfilePictureUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAtUtc { get; set; }
}
