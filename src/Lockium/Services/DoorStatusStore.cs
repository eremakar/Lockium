using System.Collections.Concurrent;
using Lockium.Models;

namespace Lockium.Services;

public sealed class DoorStatusStore
{
    private readonly ConcurrentDictionary<string, PostedDoorStatus> _latest = new();

    public void Post(PostedDoorStatus status)
    {
        var key = $"{status.DeviceId}:{status.Channel}";
        _latest[key] = status;
    }

    public IReadOnlyList<PostedDoorStatus> GetAll() =>
        _latest.Values.OrderByDescending(s => s.PostedAt).ToList();

    public IReadOnlyList<PostedDoorStatus> GetByDevice(string deviceId) =>
        _latest.Values
            .Where(s => s.DeviceId == deviceId)
            .OrderByDescending(s => s.PostedAt)
            .ToList();
}
