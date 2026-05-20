using Lockium.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Lockium.Services;

public sealed class LockiumEventHandler(IServiceScopeFactory scopeFactory) : ILockiumEventHandler
{
    public async Task OnDeviceSessionRegisteredAsync(string protocolDeviceId, CancellationToken cancellationToken)
    {
        await RunScopedAsync(svc => svc.UpsertDeviceConnectedAsync(protocolDeviceId, cancellationToken))
            .ConfigureAwait(false);
    }

    public async Task OnDeviceSessionDisconnectedAsync(string protocolDeviceId, CancellationToken cancellationToken)
    {
        await RunScopedAsync(svc => svc.MarkDeviceDisconnectedAsync(protocolDeviceId, cancellationToken))
            .ConfigureAwait(false);
    }

    public async Task OnReadAllLockStatusCompletedAsync(
        string protocolDeviceId,
        AllLockStatusResult result,
        CancellationToken cancellationToken)
    {
        await RunScopedAsync(svc => svc.SyncChannelsFromReadAllAsync(protocolDeviceId, result, cancellationToken))
            .ConfigureAwait(false);
    }

    private async Task RunScopedAsync(Func<IDeviceService, Task> action)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var svc = scope.ServiceProvider.GetRequiredService<IDeviceService>();
        await action(svc).ConfigureAwait(false);
    }
}
