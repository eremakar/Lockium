namespace Lockium.Client.Api.Models.Orders;

public sealed class ShipmentCreateRequest
{
    public string? TrackingNumber { get; set; }
    public string? Size { get; set; }
    public object? Recipient { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public long? LockerId { get; set; }
    public long? CellId { get; set; }
    public long? ChannelId { get; set; }
}

public sealed class ShipmentCreateResponse
{
    public long OrderId { get; set; }
    public long? CellId { get; set; }
    public string? PinCode { get; set; }
}

public sealed class PickupRequest
{
    public string Pin { get; set; } = "";
}

public sealed class OrderLockOpenResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public long OrderId { get; set; }
    public long? ChannelId { get; set; }
}

public sealed class OrderOperationResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public long OrderId { get; set; }
    public int State { get; set; }
}
