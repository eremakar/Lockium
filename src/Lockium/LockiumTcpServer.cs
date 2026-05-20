using System.Buffers.Binary;
using System.Collections.Concurrent;
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
    LockiumProtocolFileLogger protocolLogger,
    ILockiumEventHandler lockiumEventHandler,
    IServiceProvider serviceProvider,
    ILogger<LockiumTcpServer> logger)
{
    private readonly LockBoardOptions _options = options.Value;
    private readonly ConcurrentDictionary<Guid, TcpClient> _activeClients = new();

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        int port = _options.TcpPort;
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        logger.LogInformation("Lock TCP server listening on port {Port}", port);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    client = await listener.AcceptTcpClientAsync(cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                _ = HandleClientAsync(client, cancellationToken);
            }
        }
        finally
        {
            listener.Stop();
            DisconnectAllClients("application shutdown");
        }
    }

    private void DisconnectAllClients(string reason)
    {
        foreach (var (id, client) in _activeClients.ToArray())
        {
            if (!_activeClients.TryRemove(id, out var tracked) || !ReferenceEquals(tracked, client))
                continue;

            try
            {
                if (client.Connected)
                    client.Client.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Shutdown socket failed for tracked client {ClientId}", id);
            }

            try
            {
                client.Close();
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Close failed for tracked client {ClientId}", id);
            }

            logger.LogInformation("Proactively closed TCP client {ClientId} ({Reason})", id, reason);
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        var connectionId = Guid.NewGuid();
        _activeClients[connectionId] = client;

        var remote = client.Client.RemoteEndPoint?.ToString() ?? "?";
        logger.LogInformation("[{Remote}] connected", remote);
        protocolLogger.LogConnection(remote, connected: true);

        LockBoardSession? session = null;
        string? registeredDeviceId = null;
        string? disconnectReason = null;

        try
        {
            await using var stream = client.GetStream();
            session = ActivatorUtilities.CreateInstance<LockBoardSession>(
                serviceProvider, stream, remote);

            bool handshakeComplete = false;
            using var handshakeCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            handshakeCts.CancelAfter(_options.InitialHandshakeTimeout);

            using var idleCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            while (!cancellationToken.IsCancellationRequested && client.Connected)
            {
                CancellationToken readToken = handshakeComplete
                    ? ArmIdleTimeout(idleCts)
                    : handshakeCts.Token;

                byte[]? frameBytes;
                try
                {
                    frameBytes = await ReadFrameAsync(stream, readToken);
                }
                catch (OperationCanceledException) when (!handshakeComplete && !cancellationToken.IsCancellationRequested)
                {
                    logger.LogWarning(
                        "[{Remote}] handshake timeout ({Timeout}s): no Registration/Heartbeat",
                        remote,
                        _options.InitialHandshakeTimeout.TotalSeconds);
                    protocolLogger.LogError(
                        remote,
                        session.DeviceId,
                        new TimeoutException(
                            $"No Registration or Heartbeat within {_options.InitialHandshakeTimeout.TotalSeconds:F0}s"));
                    break;
                }
                catch (OperationCanceledException) when (handshakeComplete && !cancellationToken.IsCancellationRequested)
                {
                    disconnectReason =
                        $"disconnected due to {_options.ConnectionIdleTimeout.TotalSeconds:F0}s idle timeout (no data received)";
                    logger.LogWarning("[{Remote}] {Reason}", remote, disconnectReason);
                    protocolLogger.LogError(
                        remote,
                        session.DeviceId,
                        new TimeoutException(disconnectReason));
                    break;
                }

                if (frameBytes is null)
                    break;

                if (handshakeComplete)
                    ArmIdleTimeout(idleCts);

                if (!LockiumProtocol.TryParseFrame(frameBytes, out var frame))
                {
                    logger.LogWarning("[{Remote}] invalid frame: {Frame}", remote, LockiumProtocol.FormatHex(frameBytes));
                    protocolLogger.LogInvalidFrame(remote, session?.DeviceId, frameBytes);
                    continue;
                }

                LogFrame(remote, frame);
                protocolLogger.LogRx(remote, session?.DeviceId, frame);
                session.OnFrameReceived(frame);

                if (frame.IsHeartbeat)
                {
                    handshakeComplete = true;
                    byte status = HandleHeartbeat(frame);
                    var response = LockiumProtocol.BuildHeartbeatResponse(frame.Instruction, status);
                    string note = $"heartbeat_ack status=0x{status:X2} ({(status == LockiumProtocol.StatusOk ? "OK" : "FAIL")})";
                    await session.WriteFrameAsync(response, cancellationToken, note);

                    registeredDeviceId = await TryRegisterFromDeviceIdAsync(
                        session,
                        frame.Data,
                        registry,
                        registeredDeviceId,
                        remote,
                        cancellationToken)
                        .ConfigureAwait(false);
                }
                else if (frame.IsRegister)
                {
                    handshakeComplete = true;
                    byte status = HandleRegister(frame);
                    var response = LockiumProtocol.BuildRegisterResponse(frame.Instruction, status);
                    string note = $"register_ack status=0x{status:X2} ({(status == LockiumProtocol.StatusOk ? "OK" : "FAIL")})";
                    await session.WriteFrameAsync(response, cancellationToken, note);

                    if (status == LockiumProtocol.StatusFail)
                        break;

                    registeredDeviceId = await TryRegisterFromDeviceIdAsync(
                        session,
                        frame.Data,
                        registry,
                        registeredDeviceId,
                        remote,
                        cancellationToken)
                        .ConfigureAwait(false);
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
            protocolLogger.LogError(remote, session?.DeviceId, ex);
        }
        finally
        {
            _activeClients.TryRemove(connectionId, out _);

            var disconnectedDeviceId = registeredDeviceId;
            if (disconnectedDeviceId is not null)
                registry.Unregister(disconnectedDeviceId);

            if (disconnectedDeviceId is not null)
                await InvokeLockiumHandlerSafeAsync(
                        ct => lockiumEventHandler.OnDeviceSessionDisconnectedAsync(disconnectedDeviceId, ct),
                        CancellationToken.None)
                    .ConfigureAwait(false);

            try
            {
                if (client.Connected)
                    client.Client.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "[{Remote}] socket shutdown failed", remote);
            }

            client.Dispose();
            if (disconnectReason is not null)
                logger.LogInformation("[{Remote}] {Reason}", remote, disconnectReason);
            else
                logger.LogInformation("[{Remote}] disconnected", remote);

            protocolLogger.LogConnection(
                remote,
                connected: false,
                disconnectedDeviceId,
                disconnectReason ?? "TCP client closed");
        }
    }

    private CancellationToken ArmIdleTimeout(CancellationTokenSource idleCts)
    {
        idleCts.CancelAfter(_options.ConnectionIdleTimeout);
        return idleCts.Token;
    }

    private async Task<string?> TryRegisterFromDeviceIdAsync(
        LockBoardSession session,
        byte[] data,
        LockConnectionRegistry registry,
        string? registeredDeviceId,
        string remote,
        CancellationToken cancellationToken)
    {
        string? deviceId = LockiumProtocol.TryGetDeviceId(data);
        if (deviceId is null)
            return registeredDeviceId;

        session.SetDeviceId(deviceId);

        if (registeredDeviceId == deviceId)
            return registeredDeviceId;

        if (registeredDeviceId is not null)
        {
            string previousId = registeredDeviceId;
            registry.Unregister(previousId);
            await InvokeLockiumHandlerSafeAsync(
                    ct => lockiumEventHandler.OnDeviceSessionDisconnectedAsync(previousId, ct),
                    cancellationToken)
                .ConfigureAwait(false);
        }

        registry.Register(deviceId, session);
        protocolLogger.LogDeviceRegistered(remote, deviceId);
        await InvokeLockiumHandlerSafeAsync(
                ct => lockiumEventHandler.OnDeviceSessionRegisteredAsync(deviceId, ct),
                cancellationToken)
            .ConfigureAwait(false);

        return deviceId;
    }

    private async Task InvokeLockiumHandlerSafeAsync(Func<CancellationToken, Task> invoke, CancellationToken cancellationToken)
    {
        try
        {
            await invoke(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "LockiumEventHandler (TCP) failed");
        }
    }

    private static async Task<byte[]?> ReadFrameAsync(Stream stream, CancellationToken cancellationToken)
    {
        var header = new byte[LockiumProtocol.FrameHeaderLength];
        if (!await ReadExactlyAsync(stream, header, header.Length, cancellationToken))
            return null;

        if (!header.AsSpan(0, LockiumProtocol.Magic.Length).SequenceEqual(LockiumProtocol.Magic))
            return null;

        int totalLength = BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(LockiumProtocol.Magic.Length));
        if (totalLength < LockiumProtocol.MinFrameLength || totalLength > LockiumProtocol.MaxFrameLength)
            return null;

        var frame = new byte[totalLength];
        header.CopyTo(frame, 0);

        int payloadLength = totalLength - LockiumProtocol.FrameHeaderLength;
        if (payloadLength > 0
            && !await ReadExactlyAsync(stream, frame.AsMemory(LockiumProtocol.FrameHeaderLength), payloadLength, cancellationToken))
            return null;

        return frame;
    }

    private static async Task<bool> ReadExactlyAsync(
        Stream stream,
        Memory<byte> buffer,
        int count,
        CancellationToken cancellationToken)
    {
        if (count == 0)
            return true;

        if (count > buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(count));

        int offset = 0;
        while (offset < count)
        {
            int read = await stream.ReadAsync(buffer.Slice(offset, count - offset), cancellationToken);
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
