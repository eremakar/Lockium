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

    public const byte StatusOk = 0x00;
    public const byte StatusFail = 0xFF;

    public const byte LockDoorOpen = 0x00;
    public const byte LockDoorClosed = 0x01;
    public const byte LockOutOfBounds = 0xFF;

    public const int MaxOrderNumberLength = 24;

    public const int DeviceIdLength = 8;
    public const int DeviceTypeLength = 2;
    public const int CcidLength = 20;

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

        int totalLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[Magic.Length..]);
        if (totalLength < Magic.Length + 2 + 2 || buffer.Length < totalLength)
            return false;

        buffer = buffer[..totalLength];
        if (ComputeXor(buffer[..^1]) != buffer[^1])
            return false;

        var body = buffer[(Magic.Length + 2)..^1];
        if (body.Length == 0)
            return false;

        byte instruction = body[0];
        byte[] data = body.Length > 1 ? body[1..].ToArray() : [];

        frame = new LockiumFrame((ushort)totalLength, instruction, data, buffer.ToArray());
        return true;
    }

    public static byte[] BuildFrame(byte instruction, ReadOnlySpan<byte> data)
    {
        int totalLength = Magic.Length + 2 + 1 + data.Length + 1;
        var frame = new byte[totalLength];
        Magic.CopyTo(frame);
        BinaryPrimitives.WriteUInt16LittleEndian(frame.AsSpan(4, 2), (ushort)totalLength);
        frame[6] = instruction;
        data.CopyTo(frame.AsSpan(7));
        frame[^1] = ComputeXor(frame.AsSpan(0, totalLength - 1));
        return frame;
    }

    public static byte[] BuildRegisterResponse(byte instruction, byte status) =>
        BuildFrame(instruction, [status]);

    public static byte[] BuildHeartbeatResponse(byte instruction, byte status) =>
        BuildFrame(instruction, [status]);

    public static byte[] BuildKeepChannelOpenCommand(byte channel) =>
        BuildFrame(CmdKeepChannelOpen, [channel]);

    public static byte[] BuildChannelCloseCommand(byte channel) =>
        BuildFrame(CmdChannelClose, [channel]);

    public static byte[] BuildReadSingleLockStatusCommand(byte channel) =>
        BuildFrame(CmdReadSingleLockStatus, [channel]);

    public static byte[] BuildReadAllLockStatusCommand() =>
        BuildFrame(CmdReadAllLockStatus, []);

    public static byte[] BuildOpenFewLocksCommand(ReadOnlySpan<byte> channels)
    {
        if (channels.IsEmpty)
            throw new ArgumentException("At least one channel is required.", nameof(channels));

        if (channels.Length > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(channels));

        var data = new byte[1 + channels.Length];
        data[0] = (byte)channels.Length;
        channels.CopyTo(data.AsSpan(1));
        return BuildFrame(CmdOpenFewLocks, data);
    }

    public static byte[] BuildOpenLockCommand(byte channel, ReadOnlySpan<byte> orderNumber = default)
    {
        if (orderNumber.Length > MaxOrderNumberLength)
            throw new ArgumentOutOfRangeException(nameof(orderNumber));

        var data = new byte[1 + orderNumber.Length];
        data[0] = channel;
        orderNumber.CopyTo(data.AsSpan(1));
        return BuildFrame(CmdOpenLock, data);
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

        return Encoding.ASCII.GetString(data[..DeviceIdLength]).TrimEnd('\0', ' ');
    }

    public static string FormatDoorStatusPush(ReadOnlySpan<byte> data)
    {
        if (data.Length < 2)
            return FormatHex(data);

        return $"channel={data[0]}, lock={FormatLockStatus(data[1])}";
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
            : Encoding.ASCII.GetString(order);

        return $"lock={lockText}, channel={channel}, order={orderText}";
    }
}

public readonly record struct LockiumFrame(
    ushort TotalLength,
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
}
