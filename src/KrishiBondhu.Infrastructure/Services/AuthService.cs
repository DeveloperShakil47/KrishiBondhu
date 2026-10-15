using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using KrishiBondhu.Application.Common.Interfaces;
using KrishiBondhu.Application.Common.Models;
using KrishiBondhu.Application.DTOs;
using KrishiBondhu.Application.Interfaces.Services;
using KrishiBondhu.Domain.Entities;
using KrishiBondhu.Domain.Enums;
using KrishiBondhu.Infrastructure.Data;
using KrishiBondhu.Infrastructure.Identity;

namespace KrishiBondhu.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly KrishiBondhuDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISignInTokenStore _tokenStore;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        KrishiBondhuDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ICurrentUserService currentUserService,
        ISignInTokenStore tokenStore)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _currentUserService = currentUserService;
        _tokenStore = tokenStore;
    }

    public async Task<Result<UserSessionDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        var identifier = loginDto.Identifier.Trim();
        var user = await _userManager.FindByNameAsync(identifier) 
                   ?? await _userManager.FindByEmailAsync(identifier)
                   ?? await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == identifier, cancellationToken);

        if (user == null)
        {
            return Result<UserSessionDto>.Failure("ব্যবহারকারী পাওয়া যায়নি বা পাসওয়ার্ড ভুল হয়েছে।");
        }

        if (!user.IsActive)
        {
            return Result<UserSessionDto>.Failure("আপনার অ্যাকাউন্টটি বর্তমানে নিষ্ক্রিয় রয়েছে। অনুগ্রহ করে সহায়তাকেন্দ্রের সাথে যোগাযোগ করুন।");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!passwordValid)
        {
            return Result<UserSessionDto>.Failure("ব্যবহারকারী পাওয়া যায়নি বা পাসওয়ার্ড ভুল হয়েছে।");
        }

        user.LastLoginAtUtc = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Sign in user
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? user.PhoneNumber ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("FullName", user.FullName ?? string.Empty),
            new Claim("District", user.District ?? string.Empty),
            new Claim("Upazila", user.Upazila ?? string.Empty)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = loginDto.RememberMe,
            ExpiresUtc = DateTime.UtcNow.AddDays(7)
        };

        if (_httpContextAccessor.HttpContext != null && !_httpContextAccessor.HttpContext.Response.HasStarted)
        {
            try
            {
                await _httpContextAccessor.HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);
            }
            catch (InvalidOperationException)
            {
                // Response has already started (SignalR circuit context)
            }
        }

        Guid? profileId = null;
        if (user.Role == UserRole.Farmer)
        {
            var farmer = await _context.FarmerProfiles.FirstOrDefaultAsync(f => f.UserId == user.Id, cancellationToken);
            profileId = farmer?.Id;
        }
        else if (user.Role == UserRole.Buyer)
        {
            var buyer = await _context.BuyerProfiles.FirstOrDefaultAsync(b => b.UserId == user.Id, cancellationToken);
            profileId = buyer?.Id;
        }

        var session = new UserSessionDto
        {
            UserId = user.Id,
            Username = user.UserName ?? user.PhoneNumber ?? string.Empty,
            FullName = user.FullName ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Email = user.Email,
            Role = user.Role,
            IsVerified = user.IsVerified,
            District = user.District ?? string.Empty,
            Upazila = user.Upazila ?? string.Empty,
            ProfileId = profileId
        };

        return Result<UserSessionDto>.Success(session, "সফলভাবে লগইন হয়েছে।");
    }

    public async Task<Result<string>> GenerateSignInTokenAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        var identifier = loginDto.Identifier.Trim();
        var user = await _userManager.FindByNameAsync(identifier) 
                   ?? await _userManager.FindByEmailAsync(identifier)
                   ?? await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == identifier, cancellationToken);

        if (user == null)
        {
            return Result<string>.Failure("ব্যবহারকারী পাওয়া যায়নি বা পাসওয়ার্ড ভুল হয়েছে।");
        }

        if (!user.IsActive)
        {
            return Result<string>.Failure("আপনার অ্যাকাউন্টটি বর্তমানে নিষ্ক্রিয় রয়েছে। অনুগ্রহ করে সহায়তাকেন্দ্রের সাথে যোগাযোগ করুন।");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!passwordValid)
        {
            return Result<string>.Failure("ব্যবহারকারী পাওয়া যায়নি বা পাসওয়ার্ড ভুল হয়েছে।");
        }

        user.LastLoginAtUtc = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? user.PhoneNumber ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("FullName", user.FullName ?? string.Empty),
            new Claim("District", user.District ?? string.Empty),
            new Claim("Upazila", user.Upazila ?? string.Empty)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = loginDto.RememberMe,
            ExpiresUtc = DateTime.UtcNow.AddDays(7)
        };

        var token = _tokenStore.CreateToken(new ClaimsPrincipal(claimsIdentity), authProperties);
        return Result<string>.Success(token);
    }

    public async Task<Result<string>> GenerateSignInTokenForUserAsync(Guid userId, bool isPersistent = false, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Result<string>.Failure("ব্যবহারকারী খুঁজে পাওয়া যায়নি।");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? user.PhoneNumber ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("FullName", user.FullName ?? string.Empty),
            new Claim("District", user.District ?? string.Empty),
            new Claim("Upazila", user.Upazila ?? string.Empty)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = isPersistent,
            ExpiresUtc = DateTime.UtcNow.AddDays(7)
        };

        var token = _tokenStore.CreateToken(new ClaimsPrincipal(claimsIdentity), authProperties);
        return Result<string>.Success(token);
    }

    public async Task<Result<UserSessionDto>> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default)
    {
        // Security rule: Disallow self-registration for Administrator
        if (registerDto.Role == UserRole.Administrator)
        {
            return Result<UserSessionDto>.Failure("প্রশাসক হিসেবে সরাসরি নিবন্ধন সম্ভব নয়।");
        }

        var phone = registerDto.PhoneNumber.Trim();
        var existingPhone = await _userManager.Users.AnyAsync(u => u.PhoneNumber == phone, cancellationToken);
        if (existingPhone)
        {
            return Result<UserSessionDto>.Failure("এই মোবাইল নম্বর দিয়ে ইতিমধ্যে একটি অ্যাকাউন্ট নিবন্ধিত রয়েছে।");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = phone,
            PhoneNumber = phone,
            Email = registerDto.Email?.Trim(),
            FullName = registerDto.FullName.Trim(),
            Role = registerDto.Role,
            District = registerDto.District.Trim(),
            Upazila = registerDto.Upazila.Trim(),
            Union = registerDto.Union?.Trim(),
            IsVerified = false,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, registerDto.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            return Result<UserSessionDto>.Failure($"নিবন্ধন ব্যর্থ হয়েছে: {errors}");
        }

        await _userManager.AddToRoleAsync(user, registerDto.Role.ToString());

        Guid? profileId = null;

        // Auto-provision domain profile
        if (registerDto.Role == UserRole.Farmer)
        {
            var farmerProfile = new FarmerProfile
            {
                UserId = user.Id,
                FullNameBn = user.FullName,
                FullNameEn = user.FullName,
                PhoneNumber = user.PhoneNumber,
                District = user.District,
                Upazila = user.Upazila,
                Union = user.Union ?? string.Empty,
                PreferredLanguage = "bn",
                IsActive = true
            };
            await _context.FarmerProfiles.AddAsync(farmerProfile, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            profileId = farmerProfile.Id;
        }
        else if (registerDto.Role == UserRole.Buyer)
        {
            var buyerProfile = new BuyerProfile
            {
                UserId = user.Id,
                BusinessName = user.FullName,
                ContactPersonName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                BusinessType = BuyerBusinessType.WholesalerAratdar,
                District = user.District,
                Upazila = user.Upazila,
                IsActive = true
            };
            await _context.BuyerProfiles.AddAsync(buyerProfile, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            profileId = buyerProfile.Id;
        }

        var session = new UserSessionDto
        {
            UserId = user.Id,
            Username = user.UserName,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            Role = user.Role,
            IsVerified = user.IsVerified,
            District = user.District,
            Upazila = user.Upazila,
            ProfileId = profileId
        };

        return Result<UserSessionDto>.Success(session, "আপনার অ্যাকাউন্ট সফলভাবে তৈরি হয়েছে।");
    }

    public async Task<Result> SignOutAsync()
    {
        if (_httpContextAccessor.HttpContext != null)
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        await _signInManager.SignOutAsync();
        return Result.Success();
    }

    public async Task<UserSessionDto?> GetCurrentUserSessionAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue) return null;

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null) return null;

        Guid? profileId = null;
        if (user.Role == UserRole.Farmer)
        {
            var farmer = await _context.FarmerProfiles.FirstOrDefaultAsync(f => f.UserId == user.Id, cancellationToken);
            profileId = farmer?.Id;
        }
        else if (user.Role == UserRole.Buyer)
        {
            var buyer = await _context.BuyerProfiles.FirstOrDefaultAsync(b => b.UserId == user.Id, cancellationToken);
            profileId = buyer?.Id;
        }

        return new UserSessionDto
        {
            UserId = user.Id,
            Username = user.UserName ?? user.PhoneNumber ?? string.Empty,
            FullName = user.FullName ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Email = user.Email,
            Role = user.Role,
            IsVerified = user.IsVerified,
            District = user.District ?? string.Empty,
            Upazila = user.Upazila ?? string.Empty,
            ProfileId = profileId
        };
    }
}
