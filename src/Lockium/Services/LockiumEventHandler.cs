using Lockium.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Lockium.Services;

public sealed class LockiumEventHandler(
    IServiceScopeFactory scopeFactory,
    LockiumProtocolFileLogger protocolLogger) : ILockiumEventHandler
{
    public async Task OnDeviceSessionRegisteredAsync(string protocolDeviceId, CancellationToken cancellationToken)
    {
        protocolLogger.LogDbConnection(
            null,
            protocolDeviceId,
            "handler.OnDeviceSessionRegistered",
            "invoking UpsertDeviceConnectedAsync (ConnectionState → ON)");
        await RunScopedAsync(svc => svc.UpsertDeviceConnectedAsync(protocolDeviceId, cancellationToken))
            .ConfigureAwait(false);
    }

    public async Task OnDeviceSessionDisconnectedAsync(string protocolDeviceId, CancellationToken cancellationToken)
    {
        protocolLogger.LogDbConnection(
            null,
            protocolDeviceId,
            "handler.OnDeviceSessionDisconnected",
            "invoking MarkDeviceDisconnectedAsync (ConnectionState → OFF)");
        await RunScopedAsync(svc => svc.MarkDeviceDisconnectedAsync(protocolDeviceId, cancellationToken))
            .ConfigureAwait(false);
    }

    public async Task EnsureAllDevicesDisconnectedAsync(
        DeviceHostLifecycle lifecycle,
        CancellationToken cancellationToken)
    {
        protocolLogger.LogDbConnection(
            null,
            null,
            "handler.EnsureAllDevicesDisconnected",
            $"lifecycle={lifecycle}, invoking MarkAllDevicesDisconnectedAsync");
        await RunScopedAsync(svc => svc.MarkAllDevicesDisconnectedAsync(lifecycle, cancellationToken))
            .ConfigureAwait(false);
    }

    public async Task OnReadAllLockStatusCompletedAsync(
        string protocolDeviceId,
        byte boardNumber,
        AllLockStatusResult result,
        CancellationToken cancellationToken)
    {
        await RunScopedAsync(svc => svc.SyncChannelsFromReadAllAsync(protocolDeviceId, boardNumber, result, cancellationToken))
            .ConfigureAwait(false);
    }

    public async Task OnDoorStatusPostedAsync(
        PostedDoorStatus status,
        CancellationToken cancellationToken)
    {
        await RunScopedAsync(svc => svc.SyncChannelFromDoorStatusAsync(status, cancellationToken))
            .ConfigureAwait(false);
    }

    private async Task RunScopedAsync(Func<IDeviceService, Task> action)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var svc = scope.ServiceProvider.GetRequiredService<IDeviceService>();
        await action(svc).ConfigureAwait(false);
    }
}
