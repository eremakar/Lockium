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
    private readonly ConcurrentDictionary<Guid, Task> _clientHandlers = new();

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

                var connectionId = Guid.NewGuid();
                var handlerTask = HandleClientAsync(client, connectionId, cancellationToken);
                _clientHandlers[connectionId] = handlerTask;
            }
        }
        finally
        {
            listener.Stop();

            try
            {
                await lockiumEventHandler.EnsureAllDevicesDisconnectedAsync(DeviceHostLifecycle.Stop, CancellationToken.None)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to mark all devices disconnected on application shutdown");
            }

            registry.Clear();
            DisconnectAllClients("application shutdown");
            await WaitForClientHandlersAsync().ConfigureAwait(false);
        }
    }

    private async Task WaitForClientHandlersAsync()
    {
        var tasks = _clientHandlers.Values.ToArray();
        if (tasks.Length == 0)
            return;

        try
        {
            await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(5), CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            logger.LogWarning("Timed out waiting for {Count} TCP client handlers to finish", tasks.Length);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Error while waiting for TCP client handlers");
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

    private async Task HandleClientAsync(TcpClient client, Guid connectionId, CancellationToken cancellationToken)
    {
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
                    string? frameDeviceId = LockiumProtocol.TryGetDeviceId(frame.Data);
                    LogHeartbeatDecision(remote, registeredDeviceId, frame, status, frameDeviceId);

                    var response = LockiumProtocol.BuildHeartbeatResponse(frame.Instruction, status, frame.boardNumber);
                    string note = $"heartbeat_ack status=0x{status:X2} ({(status == LockiumProtocol.StatusOk ? "OK" : "FAIL")})";
                    await session.WriteFrameAsync(response, cancellationToken, note);

                    registeredDeviceId = await TryRegisterFromDeviceIdAsync(
                        session,
                        frame.Data,
                        registry,
                        registeredDeviceId,
                        remote,
                        "heartbeat",
                        cancellationToken)
                        .ConfigureAwait(false);
                }
                else if (frame.IsRegister)
                {
                    handshakeComplete = true;
                    byte status = HandleRegister(frame);
                    string? frameDeviceId = LockiumProtocol.TryGetDeviceId(frame.Data);
                    protocolLogger.LogTcpSession(
                        "REGISTER",
                        remote,
                        frameDeviceId ?? registeredDeviceId,
                        $"""
                          ack_status: 0x{status:X2} ({(status == LockiumProtocol.StatusOk ? "OK" : "FAIL")})
                          data_length: {frame.Data.Length} (need {LockiumProtocol.DeviceIdLength + LockiumProtocol.DeviceTypeLength + LockiumProtocol.CcidLength} for register)
                          device_id_from_frame: {frameDeviceId ?? "(missing/invalid)"}
                          session_registered_id: {registeredDeviceId ?? "(none)"}
                          """.TrimEnd());

                    var response = LockiumProtocol.BuildRegisterResponse(frame.Instruction, status, frame.boardNumber);
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
                        "register",
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
            _clientHandlers.TryRemove(connectionId, out _);

            var disconnectedDeviceId = registeredDeviceId;
            if (disconnectedDeviceId is not null)
                registry.Unregister(disconnectedDeviceId);

            if (disconnectedDeviceId is not null)
            {
                protocolLogger.LogTcpSession(
                    "SESSION_DISCONNECT",
                    remote,
                    disconnectedDeviceId,
                    $"""
                      reason: {disconnectReason ?? "TCP client closed"}
                      action: registry.Unregister + DB MarkDeviceDisconnected
                      """);
                await InvokeLockiumHandlerSafeAsync(
                        "OnDeviceSessionDisconnected",
                        remote,
                        disconnectedDeviceId,
                        ct => lockiumEventHandler.OnDeviceSessionDisconnectedAsync(disconnectedDeviceId, ct),
                        CancellationToken.None)
                    .ConfigureAwait(false);
            }

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
        string trigger,
        CancellationToken cancellationToken)
    {
        string? deviceId = LockiumProtocol.TryGetDeviceId(data);
        if (deviceId is null)
        {
            protocolLogger.LogTcpSession(
                "REGISTER_SKIPPED",
                remote,
                registeredDeviceId,
                $"""
                  trigger: {trigger}
                  reason: no valid device id in frame (data_length={data.Length}, need>={LockiumProtocol.DeviceIdLength})
                  session_registered_id: {registeredDeviceId ?? "(none)"}
                  db_note: cannot update ConnectionState without valid device id
                  """);
            return registeredDeviceId;
        }

        if (session != null && registry.Get(deviceId) == null)
        {
            registry.Register(deviceId, session);
            protocolLogger.LogTcpSession(
                "REGISTER_RESTORED",
                remote,
                registeredDeviceId,
                $"""
                  trigger: {trigger}
                  reason: no valid device id in registry (data_length={data.Length}, need>={LockiumProtocol.DeviceIdLength})
                  session_registered_id: {registeredDeviceId ?? "(none)"}
                  """);
        }

        session.SetDeviceId(deviceId);

        if (registeredDeviceId == deviceId)
        {
            if (trigger != "heartbeat")
            {
                protocolLogger.LogTcpSession(
                    "REGISTER_UNCHANGED",
                    remote,
                    deviceId,
                    """
                      action: session already bound to this device id
                      db_note: register frame does not refresh ConnectionState
                      """.TrimEnd());
                return registeredDeviceId;
            }

            await RefreshConnectionStateFromHeartbeatAsync(remote, deviceId, cancellationToken)
                .ConfigureAwait(false);
            return deviceId;
        }

        if (registeredDeviceId is not null)
        {
            string previousId = registeredDeviceId;
            protocolLogger.LogTcpSession(
                "DEVICE_ID_SWITCH",
                remote,
                deviceId,
                $"""
                  trigger: {trigger}
                  previous_device_id: {previousId}
                  new_device_id: {deviceId}
                  action: unregister previous + DB disconnect previous, then register new
                  """);
            registry.Unregister(previousId);
            await InvokeLockiumHandlerSafeAsync(
                    "OnDeviceSessionDisconnected(device_switch)",
                    remote,
                    previousId,
                    ct => lockiumEventHandler.OnDeviceSessionDisconnectedAsync(previousId, ct),
                    cancellationToken)
                .ConfigureAwait(false);
        }

        registry.Register(deviceId, session);
        protocolLogger.LogDeviceRegistered(remote, deviceId);
        protocolLogger.LogTcpSession(
            "REGISTER_NEW",
            remote,
            deviceId,
            $"""
              trigger: {trigger}
              previous_session_device_id: {registeredDeviceId ?? "(none)"}
              action: registry.Register{(trigger == "heartbeat" ? " (DB refresh follows in heartbeat handler)" : " + DB UpsertDeviceConnected")}
              """);

        if (trigger != "heartbeat")
        {
            await InvokeLockiumHandlerSafeAsync(
                    "OnDeviceSessionRegistered",
                    remote,
                    deviceId,
                    ct => lockiumEventHandler.OnDeviceSessionRegisteredAsync(deviceId, ct),
                    cancellationToken)
                .ConfigureAwait(false);
        }

        if (trigger == "heartbeat")
            await RefreshConnectionStateFromHeartbeatAsync(remote, deviceId, cancellationToken)
                .ConfigureAwait(false);

        return deviceId;
    }

    private Task RefreshConnectionStateFromHeartbeatAsync(
        string remote,
        string deviceId,
        CancellationToken cancellationToken)
    {
        protocolLogger.LogTcpSession(
            "HEARTBEAT_DB_REFRESH",
            remote,
            deviceId,
            "action: UpsertDeviceConnected (ConnectionState → ON)");
        return InvokeLockiumHandlerSafeAsync(
            "OnDeviceSessionRegistered(heartbeat)",
            remote,
            deviceId,
            ct => lockiumEventHandler.OnDeviceSessionRegisteredAsync(deviceId, ct),
            cancellationToken);
    }

    private void LogHeartbeatDecision(
        string remote,
        string? registeredDeviceId,
        LockiumFrame frame,
        byte status,
        string? frameDeviceId)
    {
        bool willUpdateDb = frameDeviceId is not null;

        protocolLogger.LogTcpSession(
            "HEARTBEAT",
            remote,
            frameDeviceId ?? registeredDeviceId,
            $"""
              ack_status: 0x{status:X2} ({(status == LockiumProtocol.StatusOk ? "OK" : "FAIL")})
              data_length: {frame.Data.Length} (need>={LockiumProtocol.DeviceIdLength} for OK ack)
              device_id_from_frame: {frameDeviceId ?? "(missing/invalid)"}
              session_registered_id: {registeredDeviceId ?? "(none)"}
              will_update_db_connection_state: {willUpdateDb}
              db_note: each heartbeat with valid device id writes ConnectionState=ON
              """);
    }

    private async Task InvokeLockiumHandlerSafeAsync(
        string operation,
        string remote,
        string? deviceId,
        Func<CancellationToken, Task> invoke,
        CancellationToken cancellationToken)
    {
        protocolLogger.LogTcpSession(
            "HANDLER_BEGIN",
            remote,
            deviceId,
            $"  handler: {operation}");
        try
        {
            await invoke(cancellationToken).ConfigureAwait(false);
            protocolLogger.LogTcpSession(
                "HANDLER_OK",
                remote,
                deviceId,
                $"  handler: {operation}");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "LockiumEventHandler (TCP) {Operation} failed for {DeviceId}", operation, deviceId ?? "-");
            protocolLogger.LogError(remote, deviceId, ex);
            protocolLogger.LogTcpSession(
                "HANDLER_FAILED",
                remote,
                deviceId,
                $"  handler: {operation}\n  error: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static async Task<byte[]?> ReadFrameAsync(Stream stream, CancellationToken cancellationToken)
    {
        var header = new byte[LockiumProtocol.FrameHeaderLength];
        if (!await ReadExactlyAsync(stream, header, header.Length, cancellationToken))
            return null;

        if (!header.AsSpan(0, LockiumProtocol.Magic.Length).SequenceEqual(LockiumProtocol.Magic))
            return null;

        int totalLength = header[LockiumProtocol.Magic.Length];
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
        logger.LogInformation(
            "[{Remote}]   total={Total}, board={Board}, cmd=0x{Cmd:X2}",
            remote,
            frame.TotalLength,
            frame.boardNumber,
            frame.Instruction);

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
            _ when frame.IsReadIR => LockiumProtocol.FormatReadIrResponse(frame.Data),
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
