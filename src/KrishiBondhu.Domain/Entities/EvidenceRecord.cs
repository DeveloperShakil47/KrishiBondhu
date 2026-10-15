using KrishiBondhu.Domain.Common;

namespace KrishiBondhu.Domain.Entities;

public class EvidenceRecord : BaseEntity
{
    public string RelatedEntityType { get; set; } = string.Empty; // "Order", "WeighingRecord", "PaymentRecord", "DisputeCase"
    public Guid RelatedEntityId { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string RelativeStoragePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }

    public Guid UploadedByUserId { get; set; }
    public string? UploadedByRole { get; set; }
    public string Description { get; set; } = string.Empty;

    public bool IsConfidential { get; set; } = true;
}
