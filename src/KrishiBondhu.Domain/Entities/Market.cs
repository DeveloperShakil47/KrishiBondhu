using System;
using System.Collections.Generic;
using KrishiBondhu.Domain.Common;
using KrishiBondhu.Domain.Enums;

namespace KrishiBondhu.Domain.Entities;

public class Market : BaseEntity
{
    public string MarketCode { get; set; } = string.Empty;
    public string NameBn { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public MarketType MarketType { get; set; } = MarketType.Both;
    public string District { get; set; } = string.Empty;
    public string Upazila { get; set; } = string.Empty;
    public string? AddressDetails { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<MarketPriceObservation> Observations { get; set; } = new List<MarketPriceObservation>();
    public ICollection<ConsumerComplaint> Complaints { get; set; } = new List<ConsumerComplaint>();
}
