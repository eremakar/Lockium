namespace Lockium.Services;

/// <summary>
/// Сброс ConnectionState в БД при старте/остановке хоста.
/// Не полагается на <c>finally</c> в TCP-потоках — при закрытии консоли Windows процесс может быть убит до их выполнения.
/// </summary>
public sealed class DeviceConnectionStateHostedService(
    ILockiumEventHandler lockiumEventHandler,
    LockiumProtocolFileLogger protocolLogger,
    ILogger<DeviceConnectionStateHostedService> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        protocolLogger.LogDbConnection(
            null,
            null,
            "host.StartAsync",
            "marking all ConnectionState=ON devices as OFF (application.start)");
        logger.LogInformation("Host started: resetting all device ConnectionState to OFF in database");
        return lockiumEventHandler.EnsureAllDevicesDisconnectedAsync(DeviceHostLifecycle.Start, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Host stopping: marking all devices disconnected in database");
        await lockiumEventHandler.EnsureAllDevicesDisconnectedAsync(DeviceHostLifecycle.Stop, CancellationToken.None)
            .ConfigureAwait(false);
    }
}
