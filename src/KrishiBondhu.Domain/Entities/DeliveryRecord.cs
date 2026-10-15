using KrishiBondhu.Domain.Common;

namespace KrishiBondhu.Domain.Entities;

public class DeliveryRecord : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public string ConsignmentNumber { get; set; } = string.Empty;
    public string VehicleNumber { get; set; } = string.Empty; // e.g., "ঢাকা মেট্রো-ট ১১-৪৫৬৭"
    public string DriverName { get; set; } = string.Empty;
    public string DriverPhoneNumber { get; set; } = string.Empty;

    public DateTime? DispatchedAtUtc { get; set; }
    public DateTime? EstimatedArrivalUtc { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }

    public string PickupAddress { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;

    public bool IsReceivedByBuyer { get; set; }
    public string? BuyerReceivedNotes { get; set; }
    public string? DeliveryChallanPhotoUrl { get; set; }
}
