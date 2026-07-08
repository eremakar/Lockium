using Lockium.Models;
using Lockium.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lockium.Controllers;

[ApiController]
[Route("api/lock")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdministrator,Administrator")]
public sealed class LockController(
    LockConnectionRegistry registry,
    DoorStatusStore doorStatusStore,
    IDeviceService deviceService) : ControllerBase
{
    [HttpPost("{deviceId}/channels/{channel}/close")]
    public async Task<ActionResult<ChannelCloseResult>> ChannelClose(
        string deviceId,
        byte channel,
        [FromQuery] byte boardNumber = 0,
        CancellationToken cancellationToken = default)
    {
        var session = registry.Get(deviceId);
        if (session is null)
            return NotFound(new { message = $"Device '{deviceId}' is not connected." });

        var startedAt = DateTimeOffset.UtcNow;
        var result = await session.CloseChannelAsync(boardNumber, channel, cancellationToken);

        await LogApiCommandAsync(
            deviceId,
            LockiumProtocol.CmdChannelClose,
            channel,
            null,
            startedAt,
            result.Status,
            result.RawHex,
            new { boardNumber, result.StatusText, result.Channel },
            cancellationToken);
        result = await deviceService.EnrichChannelCloseAsync(deviceId, boardNumber, channel, result, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{deviceId}/channels/{channel}/keep-open")]
    public async Task<ActionResult<KeepChannelOpenResult>> KeepChannelOpen(
        string deviceId,
        byte channel,
        [FromQuery] byte boardNumber = 0,
        CancellationToken cancellationToken = default)
    {
        var session = registry.Get(deviceId);
        if (session is null)
            return NotFound(new { message = $"Device '{deviceId}' is not connected." });

        var startedAt = DateTimeOffset.UtcNow;
        var result = await session.KeepChannelOpenAsync(boardNumber, channel, cancellationToken);

        await LogApiCommandAsync(
            deviceId,
            LockiumProtocol.CmdKeepChannelOpen,
            channel,
            null,
            startedAt,
            result.Status,
            result.RawHex,
            new { boardNumber, result.StatusText, result.Channel },
            cancellationToken);
        result = await deviceService.EnrichKeepChannelOpenAsync(deviceId, boardNumber, channel, result, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{deviceId}/channels/open-few")]
    public async Task<ActionResult<OpenFewLocksResult>> OpenFewChannelLocks(
        string deviceId,
        [FromBody] OpenFewLocksRequest request,
        CancellationToken cancellationToken)
    {
        var session = registry.Get(deviceId);
        if (session is null)
            return NotFound(new { message = $"Device '{deviceId}' is not connected." });

        var startedAt = DateTimeOffset.UtcNow;
        var result = await session.OpenFewChannelLocksAsync(
            request.BoardNumber,
            request.Channels,
            cancellationToken);

        await LogApiCommandAsync(
            deviceId,
            LockiumProtocol.CmdOpenFewLocks,
            null,
            result.Channels,
            startedAt,
            result.Status,
            result.RawHex,
            new { request.BoardNumber, request.Channels, result.StatusText },
            cancellationToken);
        result = await deviceService.EnrichOpenFewLocksAsync(deviceId, request.BoardNumber, result, cancellationToken);
        await RefreshDoorStatusFromDevicesAsync(deviceId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{deviceId}/channels/{channel}/open")]
    public async Task<ActionResult<OpenLockResult>> OpenSingleChannelLock(
        string deviceId,
        byte channel,
        [FromBody] OpenLockRequest? request,
        CancellationToken cancellationToken)
    {
        var session = registry.Get(deviceId);
        if (session is null)
            return NotFound(new { message = $"Device '{deviceId}' is not connected." });

        var startedAt = DateTimeOffset.UtcNow;
        var boardNumber = request?.BoardNumber ?? 0;
        var result = await session.OpenSingleChannelLockAsync(
            boardNumber,
            channel,
            request?.OrderNumber,
            cancellationToken);

        await LogApiCommandAsync(
            deviceId,
            LockiumProtocol.CmdOpenLock,
            channel,
            null,
            startedAt,
            result.LockStatus,
            result.RawHex,
            new
            {
                boardNumber,
                result.LockStatusText,
                result.Channel,
                result.OrderNumber,
            },
            cancellationToken);
        result = await deviceService.EnrichOpenLockAsync(deviceId, boardNumber, channel, result, cancellationToken);
        await RefreshDoorStatusFromDevicesAsync(deviceId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{deviceId}/ir/{irId}")]
    public async Task<ActionResult<ReadIrResult>> ReadIr(
        string deviceId,
        byte irId,
        [FromQuery] byte boardNumber = 0,
        CancellationToken cancellationToken = default)
    {
        var session = registry.Get(deviceId);
        if (session is null)
            return NotFound(new { message = $"Device '{deviceId}' is not connected." });

        var startedAt = DateTimeOffset.UtcNow;
        var result = await session.ReadIrAsync(boardNumber, irId, cancellationToken);

        await LogApiCommandAsync(
            deviceId,
            LockiumProtocol.CmdReadIR,
            null,
            null,
            startedAt,
            result.Status,
            result.RawHex,
            new { boardNumber, result.IrId, result.IrValue },
            cancellationToken);
        result = await deviceService.EnrichReadIrAsync(deviceId, boardNumber, result, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{deviceId}/channels/{channel}/status")]
    public async Task<ActionResult<SingleLockStatusResult>> ReadSingleLockStatus(
        string deviceId,
        byte channel,
        [FromQuery] byte boardNumber = 0,
        CancellationToken cancellationToken = default)
    {
        var session = registry.Get(deviceId);
        if (session is null)
            return NotFound(new { message = $"Device '{deviceId}' is not connected." });

        var startedAt = DateTimeOffset.UtcNow;
        var result = await session.ReadSingleLockStatusAsync(boardNumber, channel, cancellationToken);

        await LogApiCommandAsync(
            deviceId,
            LockiumProtocol.CmdReadSingleLockStatus,
            channel,
            null,
            startedAt,
            result.Status,
            result.RawHex,
            new
            {
                boardNumber,
                result.Channel,
                result.LockStatus,
                result.LockStatusText,
            },
            cancellationToken);
        result = await deviceService.EnrichSingleLockStatusAsync(deviceId, boardNumber, result, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{deviceId}/status/all")]
    public async Task<ActionResult<AllLockStatusResult>> ReadAllChannelLockStatus(
        string deviceId,
        [FromQuery] byte boardNumber = 0,
        CancellationToken cancellationToken = default)
    {
        var session = registry.Get(deviceId);
        if (session is null)
            return NotFound(new { message = $"Device '{deviceId}' is not connected." });

        var startedAt = DateTimeOffset.UtcNow;
        var result = await session.ReadAllChannelLockStatusAsync(boardNumber, cancellationToken);

        await LogApiCommandAsync(
            deviceId,
            LockiumProtocol.CmdReadAllLockStatus,
            null,
            null,
            startedAt,
            result.Status,
            result.RawHex,
            new { boardNumber, result.ChannelCount },
            cancellationToken);
        result = await deviceService.SyncChannelsFromReadAllAsync(deviceId, boardNumber, result, cancellationToken);
        return Ok(result);
    }

    [HttpGet("door-status")]
    public async Task<ActionResult<IReadOnlyList<PostedDoorStatus>>> PostingTheDoorStatusActively(
        [FromQuery] string? deviceId,
        CancellationToken cancellationToken)
    {
        var items = await deviceService.GetDoorStatusesAsync(deviceId, cancellationToken);
        return Ok(items);
    }

    [HttpGet("devices")]
    public ActionResult<IReadOnlyList<string>> GetConnectedDevices() =>
        Ok(registry.GetConnectedDeviceIds());

    private Task LogApiCommandAsync(
        string deviceId,
        byte instruction,
        byte? channel,
        IReadOnlyList<byte>? channels,
        DateTimeOffset startedAt,
        byte status,
        string rawHex,
        object? details,
        CancellationToken cancellationToken) =>
        deviceService.LogLockApiCommandAsync(
            deviceId,
            instruction,
            channel,
            channels,
            startedAt,
            DateTimeOffset.UtcNow,
            status,
            rawHex,
            details,
            cancellationToken);

    private async Task RefreshDoorStatusFromDevicesAsync(string deviceId, CancellationToken cancellationToken)
    {
        var items = await deviceService.GetDoorStatusesAsync(deviceId, cancellationToken);
        foreach (var item in items)
            doorStatusStore.Post(item);
    }
}
