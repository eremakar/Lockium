using Lockium.Models;

namespace Lockium.Services;

public interface IDeviceService
{
    /// <summary>При регистрации сессии по TCP: создаёт/обновляет Device, ConnectionState = включен.</summary>
    Task UpsertDeviceConnectedAsync(string protocolDeviceId, CancellationToken cancellationToken);

    /// <summary>При отключении TCP: ConnectionState = выключен.</summary>
    Task MarkDeviceDisconnectedAsync(string protocolDeviceId, CancellationToken cancellationToken);

    /// <summary>После успешного ReadAllLockStatus — синхронизация строк Channels по номерам ячеек.</summary>
    Task SyncChannelsFromReadAllAsync(string protocolDeviceId, AllLockStatusResult result, CancellationToken cancellationToken);
}
