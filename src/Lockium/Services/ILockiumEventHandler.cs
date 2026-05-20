using Lockium.Models;

namespace Lockium.Services;

/// <summary>
/// Точка входа для побочных эффектов протокола (БД и т.д.): делегирует в <see cref="IDeviceService"/> внутри scope.
/// </summary>
public interface ILockiumEventHandler
{
    Task OnDeviceSessionRegisteredAsync(string protocolDeviceId, CancellationToken cancellationToken);

    Task OnDeviceSessionDisconnectedAsync(string protocolDeviceId, CancellationToken cancellationToken);

    Task OnReadAllLockStatusCompletedAsync(string protocolDeviceId, AllLockStatusResult result, CancellationToken cancellationToken);
}
