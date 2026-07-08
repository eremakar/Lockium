using System.Collections.Concurrent;

namespace Lockium.Services;

public sealed class LockConnectionRegistry
{
    private readonly ConcurrentDictionary<string, LockBoardSession> _sessions = new();

    public void Register(string deviceId, LockBoardSession session) =>
        _sessions[deviceId] = session;

    public void Unregister(string deviceId) =>
        _sessions.TryRemove(deviceId, out _);

    public LockBoardSession? Get(string deviceId) =>
        _sessions.TryGetValue(deviceId, out var session) ? session : null;

    public IReadOnlyList<string> GetConnectedDeviceIds() =>
        _sessions.Keys.OrderBy(id => id).ToList();

    public void Clear() => _sessions.Clear();
}
