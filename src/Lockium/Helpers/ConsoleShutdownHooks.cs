using System.Runtime.InteropServices;
using Lockium.Services;
using Microsoft.Extensions.Hosting;

namespace Lockium.Helpers;

/// <summary>
/// Закрытие окна консоли (крестик) на Windows не даёт нормально отработать <c>finally</c> в фоновых задачах.
/// Здесь: <see cref="IHostApplicationLifetime.ApplicationStopping"/> и CTRL_CLOSE_EVENT → <see cref="IHostApplicationLifetime.StopApplication"/>.
/// <see cref="AppDomain.ProcessExit"/> намеренно не используется: к этому моменту <see cref="IServiceProvider"/> уже disposed.
/// </summary>
public static class ConsoleShutdownHooks
{
    private const int CtrlCloseEvent = 2;
    private const int CtrlLogoffEvent = 5;
    private const int CtrlShutdownEvent = 6;

    private static int _devicesFlushDone;

    public static void Register(WebApplication app)
    {
        var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        var eventHandler = app.Services.GetRequiredService<ILockiumEventHandler>();
        var logger = app.Services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(ConsoleShutdownHooks));

        lifetime.ApplicationStopping.Register(() =>
            FlushDevicesToDatabase(eventHandler, logger, "ApplicationStopping"));

        if (OperatingSystem.IsWindows())
            RegisterWindowsConsoleCtrlHandler(lifetime, logger);
    }

    private static void FlushDevicesToDatabase(
        ILockiumEventHandler eventHandler,
        ILogger logger,
        string source)
    {
        if (Interlocked.CompareExchange(ref _devicesFlushDone, 1, 0) != 0)
            return;

        try
        {
            eventHandler.EnsureAllDevicesDisconnectedAsync(DeviceHostLifecycle.Stop, CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            TryLog(logger, () => logger.LogInformation("{Source}: all devices marked disconnected in database", source));
        }
        catch (Exception ex) when (ex is ObjectDisposedException or InvalidOperationException)
        {
            Interlocked.Exchange(ref _devicesFlushDone, 0);
        }
        catch (Exception ex)
        {
            Interlocked.Exchange(ref _devicesFlushDone, 0);
            TryLog(logger, () => logger.LogWarning(ex, "{Source}: failed to mark devices disconnected", source));
        }
    }

    private static void TryLog(ILogger logger, Action write)
    {
        try
        {
            write();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private static void RegisterWindowsConsoleCtrlHandler(
        IHostApplicationLifetime lifetime,
        ILogger logger)
    {
        ConsoleCtrlHandler handler = ctrlType =>
        {
            if (ctrlType is not (CtrlCloseEvent or CtrlLogoffEvent or CtrlShutdownEvent))
                return false;

            logger.LogInformation(
                "Windows console event {CtrlType} (0x{CtrlType:X}): requesting graceful host shutdown",
                ctrlType,
                ctrlType);

            try
            {
                lifetime.StopApplication();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "StopApplication failed for console event {CtrlType}", ctrlType);
            }

            return true;
        };

        if (!SetConsoleCtrlHandler(handler, add: true))
            logger.LogWarning("SetConsoleCtrlHandler failed; closing the console window may kill the process without DB cleanup");
    }

    private delegate bool ConsoleCtrlHandler(int ctrlType);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandler handler, bool add);
}
