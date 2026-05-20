using System.Diagnostics;
using System.Net.Sockets;
using Lockium.Models;
using Lockium.Options;
using Microsoft.Extensions.Options;

namespace Lockium.Services;

public sealed class LockBoardSession(
    NetworkStream stream,
    string remoteEndPoint,
    IOptions<LockBoardOptions> options,
    DoorStatusStore doorStatusStore,
    LockiumProtocolFileLogger protocolLogger,
    ILogger<LockBoardSession> logger)
{
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly SemaphoreSlim _commandLock = new(1, 1);
    private readonly LockBoardOptions _options = options.Value;

    private TaskCompletionSource<LockiumFrame>? _pendingResponse;
    private byte _expectedInstruction;

    public string? DeviceId { get; private set; }
    public string RemoteEndPoint => remoteEndPoint;

    public void SetDeviceId(string deviceId) => DeviceId = deviceId;

    public async Task WriteFrameAsync(byte[] frame, CancellationToken cancellationToken, string? note = null)
    {
        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            logger.LogInformation("[{Remote}] TX: {Frame}", remoteEndPoint, LockiumProtocol.FormatHex(frame));
            protocolLogger.LogTx(remoteEndPoint, DeviceId, frame, note);
            await stream.WriteAsync(frame, cancellationToken);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task<ChannelCloseResult> CloseChannelAsync(
        byte channel,
        CancellationToken cancellationToken)
    {
        var command = LockiumProtocol.BuildChannelCloseCommand(channel);
        var frame = await SendAndWaitAsync(command, LockiumProtocol.CmdChannelClose, cancellationToken);

        byte status = frame.Data.Length > 0 ? frame.Data[0] : (byte)0;
        byte respChannel = frame.Data.Length > 1 ? frame.Data[1] : channel;

        return new ChannelCloseResult(
            status,
            status == LockiumProtocol.StatusOk ? "OK" : $"0x{status:X2}",
            respChannel,
            LockiumProtocol.FormatHex(frame.Raw));
    }

    public async Task<KeepChannelOpenResult> KeepChannelOpenAsync(
        byte channel,
        CancellationToken cancellationToken)
    {
        var command = LockiumProtocol.BuildKeepChannelOpenCommand(channel);
        var frame = await SendAndWaitAsync(command, LockiumProtocol.CmdKeepChannelOpen, cancellationToken);

        byte status = frame.Data.Length > 0 ? frame.Data[0] : (byte)0;
        byte respChannel = frame.Data.Length > 1 ? frame.Data[1] : channel;

        return new KeepChannelOpenResult(
            status,
            status == LockiumProtocol.StatusOk ? "OK" : $"0x{status:X2}",
            respChannel,
            LockiumProtocol.FormatHex(frame.Raw));
    }

    public async Task<OpenFewLocksResult> OpenFewChannelLocksAsync(
        IReadOnlyList<byte> channels,
        CancellationToken cancellationToken)
    {
        if (channels.Count == 0)
            throw new ArgumentException("At least one channel is required.", nameof(channels));

        var command = LockiumProtocol.BuildOpenFewLocksCommand(channels.ToArray());
        var frame = await SendAndWaitAsync(command, LockiumProtocol.CmdOpenFewLocks, cancellationToken);

        byte status = frame.Data.Length > 0 ? frame.Data[0] : (byte)0;
        return new OpenFewLocksResult(
            status,
            status == LockiumProtocol.StatusOk ? "OK" : $"0x{status:X2}",
            channels,
            LockiumProtocol.FormatHex(frame.Raw));
    }

    public async Task<OpenLockResult> OpenSingleChannelLockAsync(
        byte channel,
        string? orderNumber,
        CancellationToken cancellationToken)
    {
        byte[] orderBytes = string.IsNullOrEmpty(orderNumber)
            ? []
            : System.Text.Encoding.ASCII.GetBytes(orderNumber);

        var command = LockiumProtocol.BuildOpenLockCommand(channel, orderBytes);
        var frame = await SendAndWaitAsync(command, LockiumProtocol.CmdOpenLock, cancellationToken);
        RecordDoorStatusFromOpenLock(frame);

        byte lockStatus = frame.Data.Length > 0 ? frame.Data[0] : (byte)0;
        byte respChannel = frame.Data.Length > 1 ? frame.Data[1] : channel;
        string? respOrder = frame.Data.Length > 2
            ? System.Text.Encoding.ASCII.GetString(frame.Data[2..])
            : orderNumber;

        return new OpenLockResult(
            lockStatus,
            LockiumProtocol.FormatLockStatus(lockStatus),
            respChannel,
            respOrder,
            LockiumProtocol.FormatHex(frame.Raw));
    }

    public async Task<SingleLockStatusResult> ReadSingleLockStatusAsync(
        byte channel,
        CancellationToken cancellationToken)
    {
        var command = LockiumProtocol.BuildReadSingleLockStatusCommand(channel);
        var frame = await SendAndWaitAsync(command, LockiumProtocol.CmdReadSingleLockStatus, cancellationToken);
        RecordDoorStatusFromReadSingle(frame);

        byte lockStatus = frame.Data.Length > 2 ? frame.Data[2] : (byte)0;
        return new SingleLockStatusResult(
            frame.Data.Length > 0 ? frame.Data[0] : (byte)0,
            frame.Data.Length > 1 ? frame.Data[1] : channel,
            lockStatus,
            LockiumProtocol.FormatLockStatus(lockStatus),
            LockiumProtocol.FormatHex(frame.Raw));
    }

    public async Task<AllLockStatusResult> ReadAllChannelLockStatusAsync(CancellationToken cancellationToken)
    {
        var command = LockiumProtocol.BuildReadAllLockStatusCommand();
        var frame = await SendAndWaitAsync(command, LockiumProtocol.CmdReadAllLockStatus, cancellationToken);
        RecordDoorStatusFromReadAll(frame);

        byte status = frame.Data.Length > 0 ? frame.Data[0] : (byte)0;
        byte count = frame.Data.Length > 1 ? frame.Data[1] : (byte)0;
        var locks = frame.Data.Length > 2 ? frame.Data[2..].ToArray() : [];

        return new AllLockStatusResult(
            status,
            count,
            locks,
            locks.Select(LockiumProtocol.FormatLockStatus).ToList(),
            LockiumProtocol.FormatHex(frame.Raw));
    }

    public void OnFrameReceived(LockiumFrame frame)
    {
        if (_pendingResponse is not null && frame.Instruction == _expectedInstruction)
        {
            _pendingResponse.TrySetResult(frame);
            return;
        }

        if (_pendingResponse is not null)
            protocolLogger.LogUnsolicitedFrame(remoteEndPoint, DeviceId, frame, _expectedInstruction);

        if (frame.IsDoorStatusPush)
            RecordDoorStatusFromPush(frame);
    }

    private async Task<LockiumFrame> SendAndWaitAsync(
        byte[] command,
        byte expectedInstruction,
        CancellationToken cancellationToken,
        string? apiContext = null)
    {
        await _commandLock.WaitAsync(cancellationToken);
        var sw = Stopwatch.StartNew();
        try
        {
            var tcs = new TaskCompletionSource<LockiumFrame>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingResponse = tcs;
            _expectedInstruction = expectedInstruction;

            protocolLogger.LogCommandRequest(
                remoteEndPoint,
                DeviceId,
                expectedInstruction,
                command,
                apiContext ?? LockiumProtocol.GetCommandName(expectedInstruction));

            await WriteFrameAsync(command, cancellationToken);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_options.CommandTimeout);

            try
            {
                var response = await tcs.Task.WaitAsync(timeoutCts.Token);
                sw.Stop();
                protocolLogger.LogCommandResponse(
                    remoteEndPoint,
                    DeviceId,
                    expectedInstruction,
                    response,
                    sw.Elapsed,
                    matchedPending: true);
                return response;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                sw.Stop();
                protocolLogger.LogCommandTimeout(remoteEndPoint, DeviceId, expectedInstruction, sw.Elapsed);
                throw;
            }
        }
        finally
        {
            _pendingResponse = null;
            _commandLock.Release();
        }
    }

    private void RecordDoorStatusFromPush(LockiumFrame frame)
    {
        if (DeviceId is null || frame.Data.Length < 2)
            return;

        PostStatus(DeviceId, frame.Data[0], frame.Data[1], frame.Instruction, frame.Raw);
    }

    private void RecordDoorStatusFromReadSingle(LockiumFrame frame)
    {
        if (DeviceId is null || frame.Data.Length < 3)
            return;

        PostStatus(DeviceId, frame.Data[1], frame.Data[2], frame.Instruction, frame.Raw);
    }

    private void RecordDoorStatusFromOpenLock(LockiumFrame frame)
    {
        if (DeviceId is null || frame.Data.Length < 2)
            return;

        PostStatus(DeviceId, frame.Data[1], frame.Data[0], frame.Instruction, frame.Raw);
    }

    private void RecordDoorStatusFromReadAll(LockiumFrame frame)
    {
        if (DeviceId is null || frame.Data.Length < 3)
            return;

        byte count = frame.Data[1];
        for (int i = 0; i < count && i + 2 < frame.Data.Length; i++)
            PostStatus(DeviceId, (byte)(i + 1), frame.Data[i + 2], frame.Instruction, frame.Raw);
    }

    private void PostStatus(string deviceId, byte channel, byte lockStatus, byte command, byte[] raw)
    {
        doorStatusStore.Post(new PostedDoorStatus(
            deviceId,
            channel,
            lockStatus,
            LockiumProtocol.FormatLockStatus(lockStatus),
            command,
            DateTimeOffset.UtcNow,
            LockiumProtocol.FormatHex(raw)));
    }
}
