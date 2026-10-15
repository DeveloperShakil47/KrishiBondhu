using System;
using System.ComponentModel.DataAnnotations;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Application.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "মোবাইল নম্বর বা ইমেইল প্রদান করুন")]
    public string Identifier { get; set; } = string.Empty;

    [Required(ErrorMessage = "পাসওয়ার্ড প্রদান করুন")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public class RegisterDto
{
    [Required(ErrorMessage = "পূর্ণ নাম প্রদান করুন")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "সঠিক মোবাইল নম্বর প্রদান করুন")]
    [Phone(ErrorMessage = "১১ ডিজিটের সঠিক বাংলাদেশি মোবাইল নম্বর দিন")]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "সঠিক ইমেইল ঠিকানা দিন (ঐচ্ছিক)")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "পাসওয়ার্ড প্রদান করুন")]
    [MinLength(6, ErrorMessage = "পাসওয়ার্ড ন্যূনতম ৬ অক্ষরের হতে হবে")]
    public string Password { get; set; } = string.Empty;

    [Compare(nameof(Password), ErrorMessage = "পাসওয়ার্ড দুটি মিলছে না")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "ভূমিকা নির্বাচন করুন")]
    public UserRole Role { get; set; } = UserRole.Farmer;

    [Required(ErrorMessage = "জেলা নির্বাচন করুন")]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "উপজেলা নির্বাচন করুন")]
    public string Upazila { get; set; } = string.Empty;

    public string? Union { get; set; }
}

public class UserSessionDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public UserRole Role { get; set; }
    public bool IsVerified { get; set; }
    public string District { get; set; } = string.Empty;
    public string Upazila { get; set; } = string.Empty;
    public Guid? ProfileId { get; set; }
}
