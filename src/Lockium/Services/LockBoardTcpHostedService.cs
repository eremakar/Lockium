namespace Lockium.Services;

public sealed class LockBoardTcpHostedService(LockiumTcpServer tcpServer) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        tcpServer.RunAsync(stoppingToken);
}
