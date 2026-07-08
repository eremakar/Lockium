using Lockium.Models.Dtos.Devices;

namespace Lockium.Models;

public sealed record OpenLockRequest(string? OrderNumber = null, byte BoardNumber = 0);

public sealed record OpenFewLocksRequest(IReadOnlyList<byte> Channels, byte BoardNumber = 0);

public sealed record ChannelCloseResult(
    byte Status,
    string StatusText,
    byte Channel,
    string RawHex,
    BoardDto? Board = null,
    ChannelDto? LinkedChannel = null);

public sealed record KeepChannelOpenResult(
    byte Status,
    string StatusText,
    byte Channel,
    string RawHex,
    BoardDto? Board = null,
    ChannelDto? LinkedChannel = null);

public sealed record OpenFewLocksResult(
    byte Status,
    string StatusText,
    IReadOnlyList<byte> Channels,
    string RawHex,
    BoardDto? Board = null,
    IReadOnlyList<ChannelDto>? LinkedChannels = null);

public sealed record OpenLockResult(
    byte LockStatus,
    string LockStatusText,
    byte Channel,
    string? OrderNumber,
    string RawHex,
    BoardDto? Board = null,
    ChannelDto? LinkedChannel = null);

public sealed record ReadIrResult(
    byte Status,
    byte IrId,
    byte IrValue,
    string RawHex,
    BoardDto? Board = null,
    IRChannelDto? IrChannel = null);

public sealed record SingleLockStatusResult(
    byte Status,
    byte Channel,
    byte LockStatus,
    string LockStatusText,
    string RawHex,
    BoardDto? Board = null,
    ChannelDto? LinkedChannel = null);

public sealed record AllLockStatusResult(
    byte Status,
    int ChannelCount,
    IReadOnlyList<byte> LockStatuses,
    IReadOnlyList<string> LockStatusTexts,
    string RawHex,
    BoardDto? Board = null,
    IReadOnlyList<ChannelDto>? Channels = null);

public sealed record PostedDoorStatus(
    string DeviceId,
    byte Channel,
    byte LockStatus,
    string LockStatusText,
    byte Command,
    DateTimeOffset PostedAt,
    string RawHex);
