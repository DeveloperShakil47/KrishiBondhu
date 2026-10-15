using System;

namespace KrishiBondhu.Application.DTOs;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? PerformedByUsername { get; set; }
    public string? PerformedByRole { get; set; }
    public string? DetailsJson { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class NotificationDto
{
    public Guid Id { get; set; }
    public string TitleBn { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string MessageBn { get; set; } = string.Empty;
    public string MessageEn { get; set; } = string.Empty;
    public string NotificationType { get; set; } = "Info";
    public string? TargetUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
