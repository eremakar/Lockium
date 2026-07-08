using System.Buffers.Binary;
using System.Text;

namespace Lockium;

/// <summary>WKLY (Voung) lock-board TCP protocol. Frame length = total packet size. XOR = ComputXor over all bytes except checksum.</summary>
public static class LockiumProtocol
{
    public static ReadOnlySpan<byte> Magic => "WKLY"u8;

    public const byte CmdHeartbeat = 0x80;
    public const byte CmdRegister = 0x81;
    public const byte CmdOpenLock = 0x82;
    public const byte CmdReadSingleLockStatus = 0x83;
    public const byte CmdReadAllLockStatus = 0x84;
    public const byte CmdDoorStatusPush = 0x85;
    public const byte CmdOpenFewLocks = 0x87;
    public const byte CmdKeepChannelOpen = 0x88;
    public const byte CmdChannelClose = 0x89;
    public const byte CmdReadIR = 0x73;

    public const byte StatusOk = 0x00;
    public const byte StatusFail = 0xFF;

    public const byte LockDoorOpen = 0x00;
    public const byte LockDoorClosed = 0x01;
    public const byte LockOutOfBounds = 0xFF;

    public const int MaxOrderNumberLength = 24;

    public const int DeviceIdLength = 8;
    public const int DeviceTypeLength = 2;
    public const int CcidLength = 20;

    public const int FrameHeaderLength = 4 + 1 + 1; // magic + length + board
    public const int MinFrameLength = FrameHeaderLength + 2;
    public const int MaxFrameLength = 2048;

    /// <summary>ComputXor — XOR of InData[0..Len-1].</summary>
    public static byte ComputeXor(ReadOnlySpan<byte> inData)
    {
        byte sum = 0;
        foreach (byte b in inData)
            sum ^= b;
        return sum;
    }

    public static bool TryParseFrame(ReadOnlySpan<byte> buffer, out LockiumFrame frame)
    {
        frame = default;
        if (buffer.Length < Magic.Length + 2 + 2)
            return false;

        if (!buffer.StartsWith(Magic))
            return false;

        int totalLength = buffer[Magic.Length];
        if (totalLength < Magic.Length + 2 + 2 || buffer.Length < totalLength)
            return false;

        buffer = buffer[..totalLength];
        if (ComputeXor(buffer[..^1]) != buffer[^1])
            return false;

        var body = buffer[(Magic.Length + 2)..^1];
        if (body.Length == 0)
            return false;

        var boardNumber = buffer[Magic.Length + 1];

        byte instruction = body[0];
        byte[] data = body.Length > 1 ? body[1..].ToArray() : [];

        frame = new LockiumFrame((ushort)totalLength, boardNumber, instruction, data, buffer.ToArray());
        return true;
    }

    public static byte[] BuildFrame(byte instruction, ReadOnlySpan<byte> data, byte boardNumber = 0)
    {
        int totalLength = Magic.Length + 1 + 1 + 1 + data.Length + 1;
        if (totalLength > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(data), totalLength, "Frame exceeds single-byte length field.");

        var frame = new byte[totalLength];
        Magic.CopyTo(frame);
        frame[Magic.Length] = (byte)totalLength;
        frame[Magic.Length + 1] = boardNumber;
        frame[6] = instruction;
        data.CopyTo(frame.AsSpan(7));
        frame[^1] = ComputeXor(frame.AsSpan(0, totalLength - 1));
        return frame;
    }

    public static byte[] BuildRegisterResponse(byte instruction, byte status, byte boardNumber = 0) =>
        BuildFrame(instruction, [status], boardNumber);

    public static byte[] BuildHeartbeatResponse(byte instruction, byte status, byte boardNumber = 0) =>
        BuildFrame(instruction, [status], boardNumber);

    public static byte[] BuildKeepChannelOpenCommand(byte channel, byte boardNumber = 0) =>
        BuildFrame(CmdKeepChannelOpen, [channel], boardNumber);

    public static byte[] BuildChannelCloseCommand(byte channel, byte boardNumber = 0) =>
        BuildFrame(CmdChannelClose, [channel], boardNumber);

    public static byte[] BuildReadSingleLockStatusCommand(byte channel, byte boardNumber = 0) =>
        BuildFrame(CmdReadSingleLockStatus, [channel], boardNumber);

    public static byte[] BuildReadAllLockStatusCommand(byte boardNumber = 0) =>
        BuildFrame(CmdReadAllLockStatus, [], boardNumber);

    public static byte[] BuildReadIrCommand(byte irId, byte boardNumber = 0) =>
        BuildFrame(CmdReadIR, [irId], boardNumber);

    public static byte[] BuildOpenFewLocksCommand(ReadOnlySpan<byte> channels, byte boardNumber = 0)
    {
        if (channels.IsEmpty)
            throw new ArgumentException("At least one channel is required.", nameof(channels));

        if (channels.Length > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(channels));

        var data = new byte[1 + channels.Length];
        data[0] = (byte)channels.Length;
        channels.CopyTo(data.AsSpan(1));
        return BuildFrame(CmdOpenFewLocks, data, boardNumber);
    }

    public static byte[] BuildOpenLockCommand(byte channel, ReadOnlySpan<byte> orderNumber = default, byte boardNumber = 0)
    {
        if (orderNumber.Length > MaxOrderNumberLength)
            throw new ArgumentOutOfRangeException(nameof(orderNumber));

        var data = new byte[1 + orderNumber.Length];
        data[0] = channel;
        orderNumber.CopyTo(data.AsSpan(1));
        return BuildFrame(CmdOpenLock, data, boardNumber);
    }

    public static string GetCommandName(byte instruction) =>
        instruction switch
        {
            CmdHeartbeat => "Heartbeat (0x80)",
            CmdRegister => "Register (0x81)",
            CmdOpenLock => "OpenLock (0x82)",
            CmdReadSingleLockStatus => "ReadSingleLockStatus (0x83)",
            CmdReadAllLockStatus => "ReadAllLockStatus (0x84)",
            CmdDoorStatusPush => "DoorStatusPush (0x85)",
            CmdOpenFewLocks => "OpenFewLocks (0x87)",
            CmdKeepChannelOpen => "KeepChannelOpen (0x88)",
            CmdChannelClose => "ChannelClose (0x89)",
            CmdReadIR => "ReadIR (0x73)",
            _ => $"Unknown (0x{instruction:X2})",
        };

    public static string FormatFrameDetail(LockiumFrame frame) =>
        frame switch
        {
            _ when frame.IsHeartbeat => FormatDeviceId(frame.Data),
            _ when frame.IsRegister => FormatRegisterData(frame.Data),
            _ when frame.IsOpenLock => FormatOpenLockResponse(frame.Data),
            _ when frame.IsOpenFewLocks => FormatOpenFewLocksResponse(frame.Data),
            _ when frame.IsReadAllLockStatus => FormatReadAllLockStatusResponse(frame.Data),
            _ when frame.IsReadSingleLockStatus => FormatReadSingleLockStatusResponse(frame.Data),
            _ when frame.IsDoorStatusPush => FormatDoorStatusPush(frame.Data),
            _ when frame.IsKeepChannelOpen => FormatKeepChannelOpenResponse(frame.Data),
            _ when frame.IsChannelClose => FormatChannelCloseResponse(frame.Data),
            _ when frame.IsReadIR => FormatReadIrResponse(frame.Data),
            _ => frame.Data.Length > 0 ? FormatHex(frame.Data) : "(no data)",
        };

    public static string FormatCommandPayload(byte instruction, ReadOnlySpan<byte> data) =>
        instruction switch
        {
            CmdOpenLock when data.Length > 0 =>
                $"channel={data[0]}" + (data.Length > 1
                    ? $", order={DecodeAsciiBytes(data[1..])}"
                    : ""),
            CmdReadIR when data.Length > 0 =>
                $"irId={data[0]}",
            CmdReadSingleLockStatus or CmdKeepChannelOpen or CmdChannelClose when data.Length > 0 =>
                $"channel={data[0]}",
            CmdOpenFewLocks when data.Length > 0 =>
                $"count={data[0]}, channels=[{FormatHex(data.Length > 1 ? data[1..] : ReadOnlySpan<byte>.Empty)}]",
            _ => data.Length > 0 ? FormatHex(data) : "(empty)",
        };

    public static bool TryGetBodyFromRaw(ReadOnlySpan<byte> raw, out byte instruction, out byte[] data)
    {
        instruction = 0;
        data = [];
        if (!TryParseFrame(raw, out var frame))
            return false;

        instruction = frame.Instruction;
        data = frame.Data;
        return true;
    }

    public static string FormatHex(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
            return string.Empty;

        var sb = new StringBuilder(data.Length * 3 - 1);
        for (int i = 0; i < data.Length; i++)
        {
            if (i > 0)
                sb.Append(' ');
            sb.Append(data[i].ToString("X2"));
        }
        return sb.ToString();
    }

    public static string FormatDeviceId(ReadOnlySpan<byte> data)
    {
        if (data.Length < DeviceIdLength)
            return FormatHex(data);

        return $"DeviceId={Encoding.ASCII.GetString(data[..DeviceIdLength])}";
    }

    public static string FormatRegisterData(ReadOnlySpan<byte> data)
    {
        if (data.Length < DeviceIdLength + DeviceTypeLength + CcidLength)
            return FormatHex(data);

        var deviceId = Encoding.ASCII.GetString(data[..DeviceIdLength]);
        ushort deviceType = BinaryPrimitives.ReadUInt16BigEndian(data[DeviceIdLength..]);
        var ccid = Encoding.ASCII.GetString(data[(DeviceIdLength + DeviceTypeLength)..(DeviceIdLength + DeviceTypeLength + CcidLength)]);

        return $"DeviceId={deviceId}, Type=0x{deviceType:X4}, CCID={ccid}";
    }

    public static string FormatKeepChannelOpenResponse(ReadOnlySpan<byte> data)
    {
        if (data.Length < 2)
            return FormatHex(data);

        byte status = data[0];
        byte channel = data[1];
        string statusText = status == StatusOk ? "OK" : $"0x{status:X2}";
        return $"status={statusText}, channel={channel}";
    }

    public static string FormatChannelCloseResponse(ReadOnlySpan<byte> data)
    {
        if (data.Length < 2)
            return FormatHex(data);

        byte status = data[0];
        byte channel = data[1];
        string statusText = status == StatusOk ? "OK" : $"0x{status:X2}";
        return $"status={statusText}, channel={channel}";
    }

    public static string FormatLockStatus(byte status) =>
        status switch
        {
            LockDoorOpen => "open",
            LockDoorClosed => "closed",
            LockOutOfBounds => "OOB",
            _ => $"0x{status:X2}",
        };

    public static string? TryGetDeviceId(ReadOnlySpan<byte> data)
    {
        if (data.Length < DeviceIdLength)
            return null;

        return DecodeAsciiBytesOrNull(data[..DeviceIdLength]);
    }

    /// <summary>Fixed-width ASCII field from device (order number, device id, etc.).</summary>
    public static string DecodeAsciiBytes(ReadOnlySpan<byte> data) =>
        data.IsEmpty ? string.Empty : Encoding.ASCII.GetString(data).TrimEnd('\0', ' ');

    public static string? DecodeAsciiBytesOrNull(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
            return null;

        var text = DecodeAsciiBytes(data);
        return text.Length == 0 ? null : text;
    }

    public static string? SanitizeAsciiField(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var text = value.TrimEnd('\0', ' ');
        return text.Length == 0 ? null : text;
    }

    public static string FormatDoorStatusPush(ReadOnlySpan<byte> data)
    {
        if (data.Length < 2)
            return FormatHex(data);

        return $"channel={data[0]}, lock={FormatLockStatus(data[1])}";
    }

    public static string FormatReadIrResponse(ReadOnlySpan<byte> data)
    {
        if (data.Length < 3)
            return FormatHex(data);

        byte status = data[0];
        byte irId = data[1];
        byte irValue = data[2];
        string statusText = status == StatusOk ? "OK" : $"0x{status:X2}";
        string extra = data.Length > 3 ? $", extra={FormatHex(data[3..])}" : "";
        return $"status={statusText}, irId={irId}, value=0x{irValue:X2}{extra}";
    }

    public static string FormatReadSingleLockStatusResponse(ReadOnlySpan<byte> data)
    {
        if (data.Length < 3)
            return FormatHex(data);

        byte status = data[0];
        byte channel = data[1];
        byte lockStatus = data[2];
        return $"status=0x{status:X2}, channel={channel}, lock={FormatLockStatus(lockStatus)}";
    }

    public static string FormatReadAllLockStatusResponse(ReadOnlySpan<byte> data)
    {
        if (data.Length < 2)
            return FormatHex(data);

        byte status = data[0];
        byte channelCount = data[1];
        ReadOnlySpan<byte> locks = data.Length > 2 ? data[2..] : ReadOnlySpan<byte>.Empty;

        var sb = new StringBuilder();
        sb.Append($"status=0x{status:X2}, channels={channelCount}");

        int count = Math.Min(channelCount, locks.Length);
        for (int i = 0; i < count; i++)
            sb.Append($", ch{i + 1}={FormatLockStatus(locks[i])}");

        if (locks.Length > channelCount)
            sb.Append($", extra={FormatHex(locks[channelCount..])}");

        return sb.ToString();
    }

    public static string FormatOpenFewLocksResponse(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
            return FormatHex(data);

        byte status = data[0];
        string statusText = status == StatusOk ? "OK" : $"0x{status:X2}";
        return $"status={statusText}";
    }

    public static string FormatOpenLockResponse(ReadOnlySpan<byte> data)
    {
        if (data.Length < 2)
            return FormatHex(data);

        byte lockStatus = data[0];
        byte channel = data[1];
        ReadOnlySpan<byte> order = data.Length > 2 ? data[2..] : ReadOnlySpan<byte>.Empty;

        string lockText = lockStatus switch
        {
            LockDoorOpen => "door open",
            LockDoorClosed => "door closed",
            LockOutOfBounds => "out of bounds",
            _ => FormatLockStatus(lockStatus),
        };

        string orderText = order.IsEmpty
            ? "(empty)"
            : DecodeAsciiBytes(order);

        return $"lock={lockText}, channel={channel}, order={orderText}";
    }
}

public readonly record struct LockiumFrame(
    ushort TotalLength,
    byte boardNumber,
    byte Instruction,
    byte[] Data,
    byte[] Raw)
{
    public bool IsHeartbeat => Instruction == LockiumProtocol.CmdHeartbeat;
    public bool IsRegister => Instruction == LockiumProtocol.CmdRegister;
    public bool IsOpenLock => Instruction == LockiumProtocol.CmdOpenLock;
    public bool IsReadSingleLockStatus => Instruction == LockiumProtocol.CmdReadSingleLockStatus;
    public bool IsReadAllLockStatus => Instruction == LockiumProtocol.CmdReadAllLockStatus;
    public bool IsDoorStatusPush => Instruction == LockiumProtocol.CmdDoorStatusPush;
    public bool IsOpenFewLocks => Instruction == LockiumProtocol.CmdOpenFewLocks;
    public bool IsKeepChannelOpen => Instruction == LockiumProtocol.CmdKeepChannelOpen;
    public bool IsChannelClose => Instruction == LockiumProtocol.CmdChannelClose;
    public bool IsReadIR => Instruction == LockiumProtocol.CmdReadIR;
}