using Lockium.Models;
using Lockium.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lockium.Controllers;

[ApiController]
[Route("api/lock")]
public sealed class LockController(
    LockConnectionRegistry registry,
    DoorStatusStore doorStatusStore) : ControllerBase
{
    [HttpPost("{deviceId}/channels/{channel}/close")]
    public async Task<ActionResult<ChannelCloseResult>> ChannelClose(
        string deviceId,
        byte channel,
        CancellationToken cancellationToken)
    {
        var session = registry.Get(deviceId);
        if (session is null)
            return NotFound(new { message = $"Device '{deviceId}' is not connected." });

        var result = await session.CloseChannelAsync(channel, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{deviceId}/channels/{channel}/keep-open")]
    public async Task<ActionResult<KeepChannelOpenResult>> KeepChannelOpen(
        string deviceId,
        byte channel,
        CancellationToken cancellationToken)
    {
        var session = registry.Get(deviceId);
        if (session is null)
            return NotFound(new { message = $"Device '{deviceId}' is not connected." });

        var result = await session.KeepChannelOpenAsync(channel, cancellationToken);
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

        var result = await session.OpenFewChannelLocksAsync(request.Channels, cancellationToken);
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

        var result = await session.OpenSingleChannelLockAsync(channel, request?.OrderNumber, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{deviceId}/channels/{channel}/status")]
    public async Task<ActionResult<SingleLockStatusResult>> ReadSingleLockStatus(
        string deviceId,
        byte channel,
        CancellationToken cancellationToken)
    {
        var session = registry.Get(deviceId);
        if (session is null)
            return NotFound(new { message = $"Device '{deviceId}' is not connected." });

        var result = await session.ReadSingleLockStatusAsync(channel, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{deviceId}/status/all")]
    public async Task<ActionResult<AllLockStatusResult>> ReadAllChannelLockStatus(
        string deviceId,
        CancellationToken cancellationToken)
    {
        var session = registry.Get(deviceId);
        if (session is null)
            return NotFound(new { message = $"Device '{deviceId}' is not connected." });

        var result = await session.ReadAllChannelLockStatusAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("door-status")]
    public ActionResult<IReadOnlyList<PostedDoorStatus>> PostingTheDoorStatusActively(
        [FromQuery] string? deviceId)
    {
        var items = deviceId is null
            ? doorStatusStore.GetAll()
            : doorStatusStore.GetByDevice(deviceId);

        return Ok(items);
    }

    [HttpGet("devices")]
    public ActionResult<IReadOnlyList<string>> GetConnectedDevices() =>
        Ok(registry.GetConnectedDeviceIds());
}
