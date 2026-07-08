using Lockium.Models;
using Lockium.Models.Dtos.Devices;

namespace Lockium.Services;

public interface IDeviceService
{
    /// <summary>При регистрации сессии по TCP: создаёт/обновляет Device, ConnectionState = включен.</summary>
    Task UpsertDeviceConnectedAsync(string protocolDeviceId, CancellationToken cancellationToken);

    /// <summary>При отключении TCP: ConnectionState = выключен.</summary>
    Task MarkDeviceDisconnectedAsync(string protocolDeviceId, CancellationToken cancellationToken);

    /// <summary>Сброс всех устройств в «выключен» (старт/остановка приложения, аварийное завершение).</summary>
    Task MarkAllDevicesDisconnectedAsync(DeviceHostLifecycle lifecycle, CancellationToken cancellationToken);

    /// <summary>После успешного ReadAllLockStatus — Board, Channels в БД и в ответе.</summary>
    Task<AllLockStatusResult> SyncChannelsFromReadAllAsync(
        string protocolDeviceId,
        byte boardNumber,
        AllLockStatusResult result,
        CancellationToken cancellationToken);

    Task<SingleLockStatusResult> EnrichSingleLockStatusAsync(
        string protocolDeviceId,
        byte boardNumber,
        SingleLockStatusResult result,
        CancellationToken cancellationToken);

    Task<ReadIrResult> EnrichReadIrAsync(
        string protocolDeviceId,
        byte boardNumber,
        ReadIrResult result,
        CancellationToken cancellationToken);

    Task<ChannelCloseResult> EnrichChannelCloseAsync(
        string protocolDeviceId,
        byte boardNumber,
        byte channel,
        ChannelCloseResult result,
        CancellationToken cancellationToken);

    Task<KeepChannelOpenResult> EnrichKeepChannelOpenAsync(
        string protocolDeviceId,
        byte boardNumber,
        byte channel,
        KeepChannelOpenResult result,
        CancellationToken cancellationToken);

    Task<OpenLockResult> EnrichOpenLockAsync(
        string protocolDeviceId,
        byte boardNumber,
        byte channel,
        OpenLockResult result,
        CancellationToken cancellationToken);

    Task<OpenFewLocksResult> EnrichOpenFewLocksAsync(
        string protocolDeviceId,
        byte boardNumber,
        OpenFewLocksResult result,
        CancellationToken cancellationToken);

    /// <summary>После door-status (push / read single / open lock) — синхронизация LockState одной ячейки.</summary>
    Task SyncChannelFromDoorStatusAsync(PostedDoorStatus status, CancellationToken cancellationToken);

    /// <summary>Door-status из Channels (Devices DB) по protocol deviceId или все устройства.</summary>
    Task<IReadOnlyList<PostedDoorStatus>> GetDoorStatusesAsync(string? protocolDeviceId, CancellationToken cancellationToken);

    /// <summary>Лог команды API <c>LockController</c> в DeviceLog (RecordType = команда, одна запись на вызов).</summary>
    Task LogLockApiCommandAsync(
        string protocolDeviceId,
        byte instruction,
        byte? channel,
        IReadOnlyList<byte>? channels,
        DateTimeOffset startedAt,
        DateTimeOffset completedAt,
        byte status,
        string rawHex,
        object? details,
        CancellationToken cancellationToken);
}
