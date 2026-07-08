using System.Diagnostics;
using System.Text;
using Lockium.Options;
using Microsoft.Extensions.Options;

namespace Lockium.Services;

/// <summary>Thread-safe file trace for WKLY lock-board protocol debugging.</summary>
public sealed class LockiumProtocolFileLogger : IDisposable
{
    private readonly LockBoardOptions _options;
    private readonly object _sync = new();
    private StreamWriter? _writer;
    private string? _resolvedPath;

    public LockiumProtocolFileLogger(IOptions<LockBoardOptions> options) =>
        _options = options.Value;

    public void LogConnection(
        string remoteEndPoint,
        bool connected,
        string? deviceId = null,
        string? detail = null)
    {
        string action = connected ? "CONNECTED" : "DISCONNECTED";
        WriteBlock(
            action,
            remoteEndPoint,
            deviceId,
            detail ?? (connected ? "TCP client accepted" : "TCP client closed"));
    }

    public void LogDeviceRegistered(string remoteEndPoint, string deviceId) =>
        WriteBlock(
            "DEVICE_REGISTERED",
            remoteEndPoint,
            deviceId,
            $"DeviceId={deviceId} bound to session");

    public void LogRx(string remoteEndPoint, string? deviceId, LockiumFrame frame, string? note = null) =>
        LogFrame("RX", remoteEndPoint, deviceId, frame, note);

    public void LogTx(
        string remoteEndPoint,
        string? deviceId,
        byte[] frameBytes,
        string? note = null)
    {
        if (!_options.ProtocolLogEnabled)
            return;

        var sb = new StringBuilder();
        sb.AppendLine($"  note: {note ?? "-"}");
        sb.AppendLine($"  raw ({frameBytes.Length} bytes): {LockiumProtocol.FormatHex(frameBytes)}");

        if (LockiumProtocol.TryParseFrame(frameBytes, out var frame))
        {
            byte xorCalc = LockiumProtocol.ComputeXor(frameBytes.AsSpan(0, frameBytes.Length - 1));
            byte xorActual = frameBytes[^1];
            sb.AppendLine($"  frame: total={frame.TotalLength}, board={frame.boardNumber}, instruction=0x{frame.Instruction:X2} ({LockiumProtocol.GetCommandName(frame.Instruction)})");
            sb.AppendLine($"  checksum: calc=0x{xorCalc:X2}, wire=0x{xorActual:X2}, ok={xorCalc == xorActual}");
            sb.AppendLine($"  data ({frame.Data.Length} bytes): {LockiumProtocol.FormatHex(frame.Data)}");
            sb.AppendLine($"  payload: {LockiumProtocol.FormatCommandPayload(frame.Instruction, frame.Data)}");
            sb.AppendLine($"  parsed: {LockiumProtocol.FormatFrameDetail(frame)}");
        }
        else
        {
            sb.AppendLine("  frame: (parse failed — invalid or incomplete)");
        }

        WriteBlock("TX", remoteEndPoint, deviceId, sb.ToString().TrimEnd());
    }

    public void LogInvalidFrame(string remoteEndPoint, string? deviceId, byte[] rawBytes) =>
        WriteBlock(
            "INVALID_FRAME",
            remoteEndPoint,
            deviceId,
            $"""
              raw ({rawBytes.Length} bytes): {LockiumProtocol.FormatHex(rawBytes)}
              reason: magic/XOR/length mismatch or truncated packet
              """.TrimEnd());

    public void LogCommandRequest(
        string remoteEndPoint,
        string? deviceId,
        byte instruction,
        byte[] commandFrame,
        string? apiContext = null)
    {
        if (!_options.ProtocolLogEnabled)
            return;

        var sb = new StringBuilder();
        sb.AppendLine($"  api: {apiContext ?? "-"}");
        sb.AppendLine($"  expected_response: 0x{instruction:X2} ({LockiumProtocol.GetCommandName(instruction)})");
        sb.AppendLine($"  timeout: {_options.CommandTimeout.TotalSeconds:F1}s");
        sb.AppendLine($"  command_raw: {LockiumProtocol.FormatHex(commandFrame)}");

        if (LockiumProtocol.TryParseFrame(commandFrame, out var frame))
        {
            sb.AppendLine($"  command_data: {LockiumProtocol.FormatHex(frame.Data)}");
            sb.AppendLine($"  command_payload: {LockiumProtocol.FormatCommandPayload(frame.Instruction, frame.Data)}");
        }

        WriteBlock("CMD_REQUEST", remoteEndPoint, deviceId, sb.ToString().TrimEnd());
    }

    public void LogCommandResponse(
        string remoteEndPoint,
        string? deviceId,
        byte expectedInstruction,
        LockiumFrame response,
        TimeSpan elapsed,
        bool matchedPending)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"  expected: 0x{expectedInstruction:X2} ({LockiumProtocol.GetCommandName(expectedInstruction)})");
        sb.AppendLine($"  elapsed_ms: {elapsed.TotalMilliseconds:F1}");
        sb.AppendLine($"  matched_pending: {matchedPending}");
        sb.AppendLine($"  response_raw: {LockiumProtocol.FormatHex(response.Raw)}");
        sb.AppendLine($"  response_data: {LockiumProtocol.FormatHex(response.Data)}");
        sb.AppendLine($"  response_parsed: {LockiumProtocol.FormatFrameDetail(response)}");

        WriteBlock("CMD_RESPONSE", remoteEndPoint, deviceId, sb.ToString().TrimEnd());
    }

    public void LogCommandTimeout(
        string remoteEndPoint,
        string? deviceId,
        byte expectedInstruction,
        TimeSpan elapsed)
    {
        WriteBlock(
            "CMD_TIMEOUT",
            remoteEndPoint,
            deviceId,
            $"""
              expected: 0x{expectedInstruction:X2} ({LockiumProtocol.GetCommandName(expectedInstruction)})
              elapsed_ms: {elapsed.TotalMilliseconds:F1}
              timeout: {_options.CommandTimeout.TotalSeconds:F1}s
              """.TrimEnd());
    }

    public void LogUnsolicitedFrame(
        string remoteEndPoint,
        string? deviceId,
        LockiumFrame frame,
        byte? expectedInstruction)
    {
        var sb = new StringBuilder();
        if (expectedInstruction is byte expected)
            sb.AppendLine($"  pending_expected: 0x{expected:X2} ({LockiumProtocol.GetCommandName(expected)})");
        else
            sb.AppendLine("  pending_expected: (none)");
        sb.AppendLine($"  instruction: 0x{frame.Instruction:X2} ({LockiumProtocol.GetCommandName(frame.Instruction)})");
        sb.AppendLine($"  raw: {LockiumProtocol.FormatHex(frame.Raw)}");
        sb.AppendLine($"  parsed: {LockiumProtocol.FormatFrameDetail(frame)}");

        WriteBlock("UNSOLICITED_RX", remoteEndPoint, deviceId, sb.ToString().TrimEnd());
    }

    public void LogError(string remoteEndPoint, string? deviceId, Exception ex) =>
        WriteBlock(
            "ERROR",
            remoteEndPoint,
            deviceId,
            $"{ex.GetType().Name}: {ex.Message}\n  stack: {ex.StackTrace}");

    /// <summary>TCP session lifecycle: registration, heartbeat decisions, handler invoke.</summary>
    public void LogTcpSession(
        string eventType,
        string remoteEndPoint,
        string? deviceId,
        string details) =>
        WriteBlock(eventType, remoteEndPoint, deviceId, details);

    /// <summary>ConnectionState changes and related DB reads/writes.</summary>
    public void LogDbConnection(
        string? remoteEndPoint,
        string? protocolDeviceId,
        string operation,
        string details)
    {
        var body = new StringBuilder();
        body.AppendLine($"  operation: {operation}");
        body.Append(details);
        WriteBlock("DB_CONNECTION", remoteEndPoint ?? "-", protocolDeviceId, body.ToString().TrimEnd());
    }

    public void Dispose()
    {
        lock (_sync)
        {
            _writer?.Dispose();
            _writer = null;
        }
    }

    private void LogFrame(string direction, string remoteEndPoint, string? deviceId, LockiumFrame frame, string? note)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"  note: {note ?? "-"}");
        sb.AppendLine($"  raw ({frame.Raw.Length} bytes): {LockiumProtocol.FormatHex(frame.Raw)}");
        sb.AppendLine($"  total_length: {frame.TotalLength}");
        sb.AppendLine($"  board_number: {frame.boardNumber}");
        sb.AppendLine($"  instruction: 0x{frame.Instruction:X2} — {LockiumProtocol.GetCommandName(frame.Instruction)}");
        sb.AppendLine($"  data ({frame.Data.Length} bytes): {LockiumProtocol.FormatHex(frame.Data)}");
        sb.AppendLine($"  payload: {LockiumProtocol.FormatCommandPayload(frame.Instruction, frame.Data)}");
        sb.AppendLine($"  parsed: {LockiumProtocol.FormatFrameDetail(frame)}");

        byte xorCalc = LockiumProtocol.ComputeXor(frame.Raw.AsSpan(0, frame.Raw.Length - 1));
        byte xorWire = frame.Raw[^1];
        sb.AppendLine($"  checksum: calc=0x{xorCalc:X2}, wire=0x{xorWire:X2}, ok={xorCalc == xorWire}");

        WriteBlock(direction, remoteEndPoint, deviceId, sb.ToString().TrimEnd());
    }

    private void WriteBlock(string eventType, string remoteEndPoint, string? deviceId, string body)
    {
        if (!_options.ProtocolLogEnabled)
            return;

        var header = new StringBuilder();
        header.Append(DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"));
        header.Append(" | ");
        header.Append(eventType);
        header.Append(" | remote=");
        header.Append(remoteEndPoint);
        header.Append(" | device=");
        header.Append(deviceId ?? "-");
        header.AppendLine();
        header.Append(body);

        lock (_sync)
        {
            EnsureWriter();
            _writer!.WriteLine(header);
            _writer.WriteLine(new string('-', 80));
            _writer.Flush();
        }
    }

    private void EnsureWriter()
    {
        if (_writer is not null)
            return;

        string path = ResolveLogPath();
        string? dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        _writer = new StreamWriter(
            new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read),
            Encoding.UTF8)
        {
            AutoFlush = false,
        };

        _writer.WriteLine();
        _writer.WriteLine($"=== LockBoard protocol log started {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} ===");
        _writer.WriteLine($"=== path: {path} ===");
        _writer.Flush();
    }

    private string ResolveLogPath()
    {
        if (_resolvedPath is not null)
            return _resolvedPath;

        string template = _options.ProtocolLogPath;
        string date = DateTime.Now.ToString("yyyy-MM-dd");
        string path = template.Replace("{date}", date, StringComparison.OrdinalIgnoreCase);

        if (!Path.IsPathRooted(path))
            path = Path.Combine(AppContext.BaseDirectory, path);

        return _resolvedPath = path;
    }
}
