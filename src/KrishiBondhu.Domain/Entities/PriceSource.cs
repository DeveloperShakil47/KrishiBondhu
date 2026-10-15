using System.Collections.Generic;
using KrishiBondhu.Domain.Common;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Domain.Entities;

public class PriceSource : BaseEntity
{
    public string NameBn { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public PriceSourceType SourceType { get; set; } = PriceSourceType.MarketSurvey;
    public string OrganizationName { get; set; } = string.Empty;
    public bool IsOfficialAuthority { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<MarketPriceReport> Reports { get; set; } = new List<MarketPriceReport>();
    public ICollection<MarketPriceObservation> Observations { get; set; } = new List<MarketPriceObservation>();
}
