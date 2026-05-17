namespace Lockium.Models;

public sealed record OpenLockRequest(string? OrderNumber);

public sealed record OpenFewLocksRequest(IReadOnlyList<byte> Channels);

public sealed record ChannelCloseResult(
    byte Status,
    string StatusText,
    byte Channel,
    string RawHex);

public sealed record KeepChannelOpenResult(
    byte Status,
    string StatusText,
    byte Channel,
    string RawHex);

public sealed record OpenFewLocksResult(
    byte Status,
    string StatusText,
    IReadOnlyList<byte> Channels,
    string RawHex);

public sealed record OpenLockResult(
    byte LockStatus,
    string LockStatusText,
    byte Channel,
    string? OrderNumber,
    string RawHex);

public sealed record SingleLockStatusResult(
    byte Status,
    byte Channel,
    byte LockStatus,
    string LockStatusText,
    string RawHex);

public sealed record AllLockStatusResult(
    byte Status,
    int ChannelCount,
    IReadOnlyList<byte> LockStatuses,
    IReadOnlyList<string> LockStatusTexts,
    string RawHex);

public sealed record PostedDoorStatus(
    string DeviceId,
    byte Channel,
    byte LockStatus,
    string LockStatusText,
    byte Command,
    DateTimeOffset PostedAt,
    string RawHex);
