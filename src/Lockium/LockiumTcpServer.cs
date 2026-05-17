using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using Lockium.Options;
using Lockium.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lockium;

public sealed class LockiumTcpServer(
    IOptions<LockBoardOptions> options,
    LockConnectionRegistry registry,
    IServiceProvider serviceProvider,
    ILogger<LockiumTcpServer> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        int port = options.Value.TcpPort;
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        logger.LogInformation("Lock TCP server listening on port {Port}", port);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(cancellationToken);
                _ = HandleClientAsync(client, cancellationToken);
            }
        }
        finally
        {
            listener.Stop();
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        var remote = client.Client.RemoteEndPoint?.ToString() ?? "?";
        logger.LogInformation("[{Remote}] connected", remote);

        LockBoardSession? session = null;
        string? registeredDeviceId = null;

        try
        {
            await using var stream = client.GetStream();
            session = ActivatorUtilities.CreateInstance<LockBoardSession>(
                serviceProvider, stream, remote);

            while (!cancellationToken.IsCancellationRequested && client.Connected)
            {
                byte[]? frameBytes = await ReadFrameAsync(stream, cancellationToken);
                if (frameBytes is null)
                    break;

                if (!LockiumProtocol.TryParseFrame(frameBytes, out var frame))
                {
                    logger.LogWarning("[{Remote}] invalid frame: {Frame}", remote, LockiumProtocol.FormatHex(frameBytes));
                    continue;
                }

                LogFrame(remote, frame);
                session.OnFrameReceived(frame);

                if (frame.IsHeartbeat)
                {
                    byte status = HandleHeartbeat(frame);
                    var response = LockiumProtocol.BuildHeartbeatResponse(frame.Instruction, status);
                    await stream.WriteAsync(response, cancellationToken);
                    logger.LogInformation("[{Remote}] TX: {Frame}", remote, LockiumProtocol.FormatHex(response));

                    TryRegisterFromDeviceId(session, frame.Data, registry, ref registeredDeviceId, remote);
                }
                else if (frame.IsRegister)
                {
                    byte status = HandleRegister(frame);
                    var response = LockiumProtocol.BuildRegisterResponse(frame.Instruction, status);
                    await stream.WriteAsync(response, cancellationToken);
                    logger.LogInformation("[{Remote}] TX: {Frame}", remote, LockiumProtocol.FormatHex(response));

                    if (status == LockiumProtocol.StatusFail)
                        break;

                    TryRegisterFromDeviceId(session, frame.Data, registry, ref registeredDeviceId, remote);
                }
            }
        }
        catch (EndOfStreamException)
        {
            // peer closed
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "[{Remote}] error", remote);
        }
        finally
        {
            if (registeredDeviceId is not null)
                registry.Unregister(registeredDeviceId);

            client.Dispose();
            logger.LogInformation("[{Remote}] disconnected", remote);
        }
    }

    private static void TryRegisterFromDeviceId(
        LockBoardSession session,
        byte[] data,
        LockConnectionRegistry registry,
        ref string? registeredDeviceId,
        string remote)
    {
        string? deviceId = LockiumProtocol.TryGetDeviceId(data);
        if (deviceId is null)
            return;

        session.SetDeviceId(deviceId);

        if (registeredDeviceId == deviceId)
            return;

        if (registeredDeviceId is not null)
            registry.Unregister(registeredDeviceId);

        registeredDeviceId = deviceId;
        registry.Register(deviceId, session);
    }

    private static async Task<byte[]?> ReadFrameAsync(Stream stream, CancellationToken cancellationToken)
    {
        var magic = new byte[LockiumProtocol.Magic.Length];
        if (!await ReadExactlyAsync(stream, magic, cancellationToken))
            return null;

        if (!magic.AsSpan().SequenceEqual(LockiumProtocol.Magic))
            return null;

        var lengthBytes = new byte[2];
        if (!await ReadExactlyAsync(stream, lengthBytes, cancellationToken))
            return null;

        int totalLength = BinaryPrimitives.ReadUInt16LittleEndian(lengthBytes);
        if (totalLength < LockiumProtocol.Magic.Length + lengthBytes.Length + 2)
            return null;

        int restLength = totalLength - LockiumProtocol.Magic.Length - lengthBytes.Length;
        var rest = new byte[restLength];
        if (restLength > 0 && !await ReadExactlyAsync(stream, rest, cancellationToken))
            return null;

        var frame = new byte[totalLength];
        magic.CopyTo(frame, 0);
        lengthBytes.CopyTo(frame, LockiumProtocol.Magic.Length);
        rest.CopyTo(frame, LockiumProtocol.Magic.Length + lengthBytes.Length);
        return frame;
    }

    private static async Task<bool> ReadExactlyAsync(Stream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        int offset = 0;
        while (offset < buffer.Length)
        {
            int read = await stream.ReadAsync(buffer.AsMemory(offset), cancellationToken);
            if (read == 0)
                return false;
            offset += read;
        }
        return true;
    }

    private void LogFrame(string remote, LockiumFrame frame)
    {
        logger.LogInformation("[{Remote}] RX: {Frame}", remote, LockiumProtocol.FormatHex(frame.Raw));
        logger.LogInformation("[{Remote}]   total={Total}, cmd=0x{Cmd:X2}", remote, frame.TotalLength, frame.Instruction);

        string? detail = frame switch
        {
            _ when frame.IsHeartbeat => LockiumProtocol.FormatDeviceId(frame.Data),
            _ when frame.IsRegister => LockiumProtocol.FormatRegisterData(frame.Data),
            _ when frame.IsOpenLock => LockiumProtocol.FormatOpenLockResponse(frame.Data),
            _ when frame.IsOpenFewLocks => LockiumProtocol.FormatOpenFewLocksResponse(frame.Data),
            _ when frame.IsReadAllLockStatus => LockiumProtocol.FormatReadAllLockStatusResponse(frame.Data),
            _ when frame.IsReadSingleLockStatus => LockiumProtocol.FormatReadSingleLockStatusResponse(frame.Data),
            _ when frame.IsDoorStatusPush => LockiumProtocol.FormatDoorStatusPush(frame.Data),
            _ when frame.IsKeepChannelOpen => LockiumProtocol.FormatKeepChannelOpenResponse(frame.Data),
            _ when frame.IsChannelClose => LockiumProtocol.FormatChannelCloseResponse(frame.Data),
            _ => frame.Data.Length > 0 ? LockiumProtocol.FormatHex(frame.Data) : null,
        };

        if (detail is not null)
            logger.LogInformation("[{Remote}]   {Detail}", remote, detail);
    }

    private static byte HandleHeartbeat(LockiumFrame frame) =>
        frame.Data.Length >= LockiumProtocol.DeviceIdLength
            ? LockiumProtocol.StatusOk
            : LockiumProtocol.StatusFail;

    private static byte HandleRegister(LockiumFrame frame)
    {
        int required = LockiumProtocol.DeviceIdLength
            + LockiumProtocol.DeviceTypeLength
            + LockiumProtocol.CcidLength;

        return frame.Data.Length >= required
            ? LockiumProtocol.StatusOk
            : LockiumProtocol.StatusFail;
    }
}
