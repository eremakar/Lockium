using System.Text.Json;
using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Data.LockiumDb.Entities.Devices;
using Lockium.Models;
using Lockium.Models.Dtos.Devices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lockium.Services;

public sealed class DeviceService(
    LockiumDbContext db,
    LockiumProtocolFileLogger protocolLogger,
    ILogger<DeviceService> logger) : IDeviceService
{
    /// <summary>Статус подключения в БД: 1 — выключен, 2 — включен, 3 — ошибка.</summary>
    public const int ConnectionOff = 1;

    public const int ConnectionOn = 2;

    /// <summary>Статус замка в Channels: 1 — закрыт, 2 — открыт.</summary>
    public const int LockClosed = 1;

    public const int LockOpen = 2;

    /// <summary>Состояние ячейки по умолчанию при создании: 1 — свободна.</summary>
    public const int ChannelStateFree = 1;

    /// <summary>Состояние IR-канала по умолчанию при создании.</summary>
    public const int IrChannelStateDefault = 1;

    /// <summary>Тип записи DeviceLog: 1 — команда, 2 — событие.</summary>
    public const int LogRecordTypeCommand = 1;

    public const int LogRecordTypeEvent = 2;

    /// <summary>Статус DeviceLog: 1 — успешно, 2 — ошибка.</summary>
    public const int LogStateOk = 1;

    public const int LogStateError = 2;

    public const string EventApplicationStart = "application.start";
    public const string EventApplicationStop = "application.stop";
    public const string EventDeviceConnected = "device.connected";
    public const string EventDeviceDisconnected = "device.disconnected";
    public const string EventChannelOpen = "channel.open";
    public const string EventChannelOpened = "channel.opened";
    public const string EventChannelClosed = "channel.closed";
    public const string EventChannelClose = "channel.close";
    public const string EventChannelKeepOpen = "channel.keep_open";
    public const string EventChannelReadStatus = "channel.read_status";
    public const string EventChannelSyncAll = "channel.sync_all";
    public const string EventChannelOpenFew = "channel.open_few";
    public const string EventChannelStatus = "channel.status";

    public async Task UpsertDeviceConnectedAsync(string protocolDeviceId, CancellationToken cancellationToken)
    {
        var key = NormalizeDeviceId(protocolDeviceId);
        if (key.Length == 0)
        {
            LogDbSkip(protocolDeviceId, "UpsertDeviceConnected", "normalized device id is empty");
            return;
        }

        var device = await db.Devices!.AsQueryable().FirstOrDefaultAsync(d => d.Name == key, cancellationToken);
        int previousState = device?.ConnectionState ?? -1;
        bool created = device is null;

        if (device is null)
        {
            device = new Device
            {
                Name = key,
                ConnectionState = ConnectionOn,
            };
            db.Devices!.Add(device);
        }
        else
            device.ConnectionState = ConnectionOn;

        int saved = await db.SaveChangesAsync(cancellationToken);
        AppendEventLog(device.Id, null, EventDeviceConnected, new { deviceName = key });
        int savedLogs = await db.SaveChangesAsync(cancellationToken);

        string detail =
            $"""
              protocol_device_id: {protocolDeviceId}
              db_name: {key}
              device_db_id: {device.Id}
              created: {created}
              connection_state: {FormatConnectionState(previousState)} → {FormatConnectionState(ConnectionOn)}
              save_changes_rows: {saved}, event_log_save_rows: {savedLogs}
              """;
        protocolLogger.LogDbConnection(null, key, "UpsertDeviceConnected", detail.TrimEnd());
        logger.LogInformation(
            "DB ConnectionState ON for {DeviceName} (id={DeviceId}, was={PreviousState}, created={Created})",
            key,
            device.Id,
            FormatConnectionState(previousState),
            created);
    }

    public async Task MarkDeviceDisconnectedAsync(string protocolDeviceId, CancellationToken cancellationToken)
    {
        var key = NormalizeDeviceId(protocolDeviceId);
        if (key.Length == 0)
        {
            LogDbSkip(protocolDeviceId, "MarkDeviceDisconnected", "normalized device id is empty");
            return;
        }

        var device = await db.Devices!.AsQueryable().FirstOrDefaultAsync(d => d.Name == key, cancellationToken);
        if (device is null)
        {
            LogDbSkip(key, "MarkDeviceDisconnected", "device not found in database");
            return;
        }

        int previousState = device.ConnectionState;
        device.ConnectionState = ConnectionOff;
        AppendEventLog(device.Id, null, EventDeviceDisconnected, new { deviceName = key });
        int saved = await db.SaveChangesAsync(cancellationToken);

        string detail =
            $"""
              protocol_device_id: {protocolDeviceId}
              db_name: {key}
              device_db_id: {device.Id}
              connection_state: {FormatConnectionState(previousState)} → {FormatConnectionState(ConnectionOff)}
              save_changes_rows: {saved}
              """;
        protocolLogger.LogDbConnection(null, key, "MarkDeviceDisconnected", detail.TrimEnd());
        logger.LogInformation(
            "DB ConnectionState OFF for {DeviceName} (id={DeviceId}, was={PreviousState})",
            key,
            device.Id,
            FormatConnectionState(previousState));
    }

    public async Task MarkAllDevicesDisconnectedAsync(
        DeviceHostLifecycle lifecycle,
        CancellationToken cancellationToken)
    {
        var connected = await db.Devices!
            .Where(d => d.ConnectionState == ConnectionOn)
            .ToListAsync(cancellationToken);

        string eventName = lifecycle == DeviceHostLifecycle.Start
            ? EventApplicationStart
            : EventApplicationStop;

        foreach (var device in connected)
            device.ConnectionState = ConnectionOff;

        AppendEventLog(
            null,
            null,
            eventName,
            new { devicesReset = connected.Count, deviceNames = connected.Select(d => d.Name).ToArray() });

        foreach (var device in connected)
            AppendEventLog(device.Id, null, EventDeviceDisconnected, new { deviceName = device.Name, reason = eventName });

        int saved = await db.SaveChangesAsync(cancellationToken);

        string names = connected.Count == 0
            ? "(none)"
            : string.Join(", ", connected.Select(d => d.Name));
        string detail =
            $"""
              lifecycle: {lifecycle}
              event: {eventName}
              devices_marked_off: {connected.Count}
              device_names: {names}
              save_changes_rows: {saved}
              """;
        protocolLogger.LogDbConnection(null, null, "MarkAllDevicesDisconnected", detail.TrimEnd());
        logger.LogInformation(
            "DB MarkAllDevicesDisconnected lifecycle={Lifecycle}, count={Count}, names=[{Names}]",
            lifecycle,
            connected.Count,
            names);
    }

    private void LogDbSkip(string? protocolDeviceId, string operation, string reason)
    {
        protocolLogger.LogDbConnection(
            null,
            protocolDeviceId,
            $"{operation}.skipped",
            $"  reason: {reason}");
        logger.LogWarning("DB {Operation} skipped for {DeviceId}: {Reason}", operation, protocolDeviceId ?? "-", reason);
    }

    private static string FormatConnectionState(int state) =>
        state switch
        {
            -1 => "(new)",
            ConnectionOff => $"OFF({ConnectionOff})",
            ConnectionOn => $"ON({ConnectionOn})",
            _ => $"unknown({state})",
        };

    public async Task<AllLockStatusResult> SyncChannelsFromReadAllAsync(
        string protocolDeviceId,
        byte boardNumber,
        AllLockStatusResult result,
        CancellationToken cancellationToken)
    {
        var key = NormalizeDeviceId(protocolDeviceId);
        if (key.Length == 0)
            return result;

        if (result.Status != LockiumProtocol.StatusOk)
        {
            var deviceForError = await db.Devices!.AsQueryable()
                .FirstOrDefaultAsync(d => d.Name == key, cancellationToken);
            AppendEventLog(
                deviceForError?.Id,
                null,
                EventChannelSyncAll,
                new { deviceName = key, boardNumber, status = result.Status, rawHex = result.RawHex },
                LogStateError);
            await db.SaveChangesAsync(cancellationToken);
            return result;
        }

        var device = await GetOrCreateDeviceAsync(key, cancellationToken);
        var board = await GetOrCreateBoardAsync(device, boardNumber, cancellationToken);

        var channelUpdates = new List<object>();
        var linkedChannels = new List<ChannelDto>();
        int count = Math.Min(result.ChannelCount, result.LockStatuses.Count);
        for (int i = 0; i < count; i++)
        {
            byte raw = result.LockStatuses[i];
            if (!TryMapProtocolLockToLockState(raw, out var lockState))
                continue;

            string number = ((byte)(i + 1)).ToString();
            var channel = UpsertChannelOnBoard(device, board, number, lockState);
            linkedChannels.Add(ToChannelDto(channel, device, board));
            channelUpdates.Add(new
            {
                channel = i + 1,
                lockState,
                lockStatusText = result.LockStatusTexts.ElementAtOrDefault(i),
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        AppendEventLog(
            device.Id,
            null,
            EventChannelSyncAll,
            new
            {
                deviceName = key,
                boardNumber,
                boardId = board.Id,
                channelCount = count,
                channels = channelUpdates,
                rawHex = result.RawHex,
            });
        await db.SaveChangesAsync(cancellationToken);

        return result with
        {
            Board = ToBoardDto(board, device),
            Channels = linkedChannels,
        };
    }

    public async Task<SingleLockStatusResult> EnrichSingleLockStatusAsync(
        string protocolDeviceId,
        byte boardNumber,
        SingleLockStatusResult result,
        CancellationToken cancellationToken)
    {
        int? lockState = TryMapProtocolLockToLockState(result.LockStatus, out var mapped)
            ? mapped
            : null;
        var (board, channel) = await EnsureBoardChannelAsync(
            protocolDeviceId,
            boardNumber,
            result.Channel,
            lockState,
            cancellationToken);
        return result with { Board = board, LinkedChannel = channel };
    }

    public async Task<ReadIrResult> EnrichReadIrAsync(
        string protocolDeviceId,
        byte boardNumber,
        ReadIrResult result,
        CancellationToken cancellationToken)
    {
        var (board, irChannel) = await EnsureBoardIrChannelAsync(
            protocolDeviceId,
            boardNumber,
            result.IrId,
            cancellationToken);
        return result with { Board = board, IrChannel = irChannel };
    }

    public async Task<ChannelCloseResult> EnrichChannelCloseAsync(
        string protocolDeviceId,
        byte boardNumber,
        byte channel,
        ChannelCloseResult result,
        CancellationToken cancellationToken)
    {
        var (boardDto, channelDto) = await EnsureBoardChannelAsync(
            protocolDeviceId,
            boardNumber,
            channel,
            null,
            cancellationToken);
        return result with { Board = boardDto, LinkedChannel = channelDto };
    }

    public async Task<KeepChannelOpenResult> EnrichKeepChannelOpenAsync(
        string protocolDeviceId,
        byte boardNumber,
        byte channel,
        KeepChannelOpenResult result,
        CancellationToken cancellationToken)
    {
        var (boardDto, channelDto) = await EnsureBoardChannelAsync(
            protocolDeviceId,
            boardNumber,
            channel,
            null,
            cancellationToken);
        return result with { Board = boardDto, LinkedChannel = channelDto };
    }

    public async Task<OpenLockResult> EnrichOpenLockAsync(
        string protocolDeviceId,
        byte boardNumber,
        byte channel,
        OpenLockResult result,
        CancellationToken cancellationToken)
    {
        int? lockState = TryMapProtocolLockToLockState(result.LockStatus, out var mapped)
            ? mapped
            : null;
        var (boardDto, channelDto) = await EnsureBoardChannelAsync(
            protocolDeviceId,
            boardNumber,
            channel,
            lockState,
            cancellationToken);
        return result with { Board = boardDto, LinkedChannel = channelDto };
    }

    public async Task<OpenFewLocksResult> EnrichOpenFewLocksAsync(
        string protocolDeviceId,
        byte boardNumber,
        OpenFewLocksResult result,
        CancellationToken cancellationToken)
    {
        var key = NormalizeDeviceId(protocolDeviceId);
        if (key.Length == 0)
            return result;

        var device = await GetOrCreateDeviceAsync(key, cancellationToken);
        var boardEntity = await GetOrCreateBoardAsync(device, boardNumber, cancellationToken);
        var linked = new List<ChannelDto>();

        foreach (var ch in result.Channels)
        {
            var channel = UpsertChannelOnBoard(device, boardEntity, ch.ToString(), null);
            linked.Add(ToChannelDto(channel, device, boardEntity));
        }

        await db.SaveChangesAsync(cancellationToken);

        return result with
        {
            Board = ToBoardDto(boardEntity, device),
            LinkedChannels = linked,
        };
    }

    public async Task SyncChannelFromDoorStatusAsync(
        PostedDoorStatus status,
        CancellationToken cancellationToken)
    {
        if (!TryMapProtocolLockToLockState(status.LockStatus, out var lockState))
            return;

        var key = NormalizeDeviceId(status.DeviceId);
        if (key.Length == 0)
            return;

        var device = await GetOrCreateDeviceAsync(key, cancellationToken);
        string number = status.Channel.ToString();
        UpsertChannelLock(device, number, lockState);
        await db.SaveChangesAsync(cancellationToken);

        long? channelId = await ResolveChannelIdAsync(device.Id, number, cancellationToken);
        string eventName = ResolveChannelEventName(status.Command, lockState);
        AppendEventLog(
            device.Id,
            channelId,
            eventName,
            new
            {
                deviceName = key,
                channel = status.Channel,
                lockState,
                lockStatus = status.LockStatus,
                lockStatusText = status.LockStatusText,
                command = status.Command,
                commandName = LockiumProtocol.GetCommandName(status.Command),
                postedAt = status.PostedAt,
                rawHex = status.RawHex,
            });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PostedDoorStatus>> GetDoorStatusesAsync(
        string? protocolDeviceId,
        CancellationToken cancellationToken)
    {
        IQueryable<Device> query = db.Devices!.AsQueryable().Include(d => d.Channels);
        if (!string.IsNullOrWhiteSpace(protocolDeviceId))
        {
            var key = NormalizeDeviceId(protocolDeviceId);
            query = query.Where(d => d.Name == key);
        }

        var devices = await query.ToListAsync(cancellationToken);
        var postedAt = DateTimeOffset.UtcNow;
        var items = new List<PostedDoorStatus>();

        foreach (var device in devices)
        {
            if (string.IsNullOrWhiteSpace(device.Name) || device.Channels is null)
                continue;

            foreach (var channel in device.Channels)
            {
                if (!TryParseChannelNumber(channel.Number, out byte channelNo))
                    continue;
                if (!TryMapLockStateToProtocolStatus(channel.LockState, out byte lockStatus))
                    continue;

                items.Add(new PostedDoorStatus(
                    device.Name,
                    channelNo,
                    lockStatus,
                    LockiumProtocol.FormatLockStatus(lockStatus),
                    LockiumProtocol.CmdDoorStatusPush,
                    postedAt,
                    string.Empty));
            }
        }

        return items
            .OrderByDescending(s => s.Channel)
            .ToList();
    }

    public async Task LogLockApiCommandAsync(
        string protocolDeviceId,
        byte instruction,
        byte? channel,
        IReadOnlyList<byte>? channels,
        DateTimeOffset startedAt,
        DateTimeOffset completedAt,
        byte status,
        string rawHex,
        object? details,
        CancellationToken cancellationToken)
    {
        var key = NormalizeDeviceId(protocolDeviceId);
        if (key.Length == 0)
            return;

        var device = await db.Devices!.AsQueryable()
            .FirstOrDefaultAsync(d => d.Name == key, cancellationToken);

        long? channelId = null;
        if (channel is { } ch && device is not null)
            channelId = await ResolveChannelIdAsync(device.Id, ch.ToString(), cancellationToken);

        var payload = new Dictionary<string, object?>
        {
            ["deviceName"] = key,
            ["startedAt"] = startedAt,
            ["completedAt"] = completedAt,
            ["instruction"] = instruction,
            ["commandName"] = LockiumProtocol.GetCommandName(instruction),
            ["source"] = "api.lock",
            ["status"] = status,
            ["rawHex"] = rawHex,
        };
        if (channel is not null)
            payload["channel"] = channel.Value;
        if (channels is { Count: > 0 })
            payload["channels"] = channels.ToArray();
        if (details is not null)
            payload["details"] = details;

        AppendDeviceLog(
            device?.Id,
            channelId,
            LogRecordTypeCommand,
            ResolveChannelCommandName(instruction),
            payload,
            ResolveLogState(status));
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<long?> ResolveChannelIdAsync(long deviceId, string number, CancellationToken cancellationToken)
    {
        var channel = await db.Channels!
            .AsQueryable()
            .FirstOrDefaultAsync(c => c.DeviceId == deviceId && c.Number == number, cancellationToken);
        return channel?.Id;
    }

    private static string ResolveChannelCommandName(byte command) =>
        command switch
        {
            LockiumProtocol.CmdOpenLock => EventChannelOpen,
            LockiumProtocol.CmdOpenFewLocks => EventChannelOpenFew,
            LockiumProtocol.CmdChannelClose => EventChannelClose,
            LockiumProtocol.CmdKeepChannelOpen => EventChannelKeepOpen,
            LockiumProtocol.CmdReadSingleLockStatus => EventChannelReadStatus,
            LockiumProtocol.CmdReadAllLockStatus => EventChannelSyncAll,
            _ => EventChannelStatus,
        };

    private static string ResolveChannelEventName(byte command, int lockState) =>
        command switch
        {
            LockiumProtocol.CmdDoorStatusPush when lockState == LockOpen => EventChannelOpened,
            LockiumProtocol.CmdDoorStatusPush when lockState == LockClosed => EventChannelClosed,
            _ => ResolveChannelCommandName(command),
        };

    private static int ResolveLogState(byte status) =>
        status is LockiumProtocol.StatusFail or LockiumProtocol.LockOutOfBounds
            ? LogStateError
            : LogStateOk;

    private void AppendEventLog(
        long? deviceId,
        long? channelId,
        string name,
        object? payload,
        int state = LogStateOk,
        string? errorMessage = null) =>
        AppendDeviceLog(deviceId, channelId, LogRecordTypeEvent, name, payload, state, errorMessage);

    private static string SerializeLogPayload(object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        // PostgreSQL json/jsonb rejects \u0000 in JSON text.
        return json.Replace("\\u0000", "", StringComparison.Ordinal);
    }

    private void AppendDeviceLog(
        long? deviceId,
        long? channelId,
        int recordType,
        string name,
        object? payload,
        int state = LogStateOk,
        string? errorMessage = null)
    {
        db.DeviceLogs!.Add(new DeviceLog
        {
            Time = DateTime.UtcNow,
            RecordType = recordType,
            Name = name,
            Payload = payload is null ? null : SerializeLogPayload(payload),
            State = state,
            ErrorMessage = errorMessage,
            DeviceId = deviceId,
            ChannelId = channelId,
        });
    }

    private async Task<Device> GetOrCreateDeviceAsync(string key, CancellationToken cancellationToken)
    {
        var device = await db.Devices!
            .Include(d => d.Channels)
            .FirstOrDefaultAsync(d => d.Name == key, cancellationToken);

        if (device is not null)
            return device;

        device = new Device
        {
            Name = key,
            ConnectionState = ConnectionOn,
            Channels = [],
        };
        db.Devices!.Add(device);
        await db.SaveChangesAsync(cancellationToken);
        return device;
    }

    private void UpsertChannelLock(Device device, string number, int lockState)
    {
        device.Channels ??= [];
        var channel = device.Channels.FirstOrDefault(c => c.Number == number);
        if (channel is null)
        {
            channel = new Channel
            {
                Number = number,
                DeviceId = device.Id,
                State = ChannelStateFree,
                LockState = lockState,
            };
            db.Channels!.Add(channel);
            device.Channels.Add(channel);
        }
        else
            channel.LockState = lockState;
    }

    private async Task<(BoardDto Board, ChannelDto Channel)> EnsureBoardChannelAsync(
        string protocolDeviceId,
        byte boardNumber,
        byte channelNumber,
        int? lockState,
        CancellationToken cancellationToken)
    {
        var key = NormalizeDeviceId(protocolDeviceId);
        if (key.Length == 0)
            return (null!, null!);

        var device = await GetOrCreateDeviceAsync(key, cancellationToken);
        var board = await GetOrCreateBoardAsync(device, boardNumber, cancellationToken);
        var channel = UpsertChannelOnBoard(device, board, channelNumber.ToString(), lockState);
        await db.SaveChangesAsync(cancellationToken);
        return (ToBoardDto(board, device), ToChannelDto(channel, device, board));
    }

    private async Task<(BoardDto Board, IRChannelDto IrChannel)> EnsureBoardIrChannelAsync(
        string protocolDeviceId,
        byte boardNumber,
        byte irId,
        CancellationToken cancellationToken)
    {
        var key = NormalizeDeviceId(protocolDeviceId);
        if (key.Length == 0)
            return (null!, null!);

        var device = await GetOrCreateDeviceAsync(key, cancellationToken);
        var board = await GetOrCreateBoardAsync(device, boardNumber, cancellationToken);
        var irChannel = await UpsertIrChannelOnBoardAsync(board, irId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return (ToBoardDto(board, device), ToIRChannelDto(irChannel, board, device));
    }

    private async Task<Board> GetOrCreateBoardAsync(
        Device device,
        byte boardNumber,
        CancellationToken cancellationToken)
    {
        var boardName = FormatBoardName(boardNumber);
        var board = await db.Boards!
            .AsQueryable()
            .FirstOrDefaultAsync(
                b => b.DeviceId == device.Id && b.Name == boardName,
                cancellationToken);

        if (board is not null)
            return board;

        board = new Board
        {
            Name = boardName,
            DeviceId = device.Id,
        };
        db.Boards!.Add(board);
        await db.SaveChangesAsync(cancellationToken);
        return board;
    }

    private Channel UpsertChannelOnBoard(Device device, Board board, string number, int? lockState)
    {
        device.Channels ??= [];
        var channel = device.Channels.FirstOrDefault(c =>
            c.Number == number && (c.BoardId == board.Id || c.BoardId is null));

        if (channel is null)
        {
            channel = new Channel
            {
                Number = number,
                DeviceId = device.Id,
                BoardId = board.Id,
                State = ChannelStateFree,
                LockState = lockState ?? LockClosed,
            };
            db.Channels!.Add(channel);
            device.Channels.Add(channel);
        }
        else
        {
            channel.BoardId = board.Id;
            channel.DeviceId = device.Id;
            if (lockState is not null)
                channel.LockState = lockState.Value;
        }

        return channel;
    }

    private async Task<IRChannel> UpsertIrChannelOnBoardAsync(
        Board board,
        byte irId,
        CancellationToken cancellationToken)
    {
        var number = irId.ToString();
        var irChannel = await db.IRChannels!
            .AsQueryable()
            .FirstOrDefaultAsync(
                ir => ir.BoardId == board.Id && ir.Number == number,
                cancellationToken);

        if (irChannel is not null)
            return irChannel;

        irChannel = new IRChannel
        {
            Number = number,
            BoardId = board.Id,
            State = IrChannelStateDefault,
        };
        db.IRChannels!.Add(irChannel);
        return irChannel;
    }

    private static string FormatBoardName(byte boardNumber) => boardNumber.ToString();

    private static BoardDto ToBoardDto(Board board, Device device) =>
        new()
        {
            Id = board.Id,
            Name = board.Name,
            DeviceId = board.DeviceId,
            UpId = board.UpId,
            Device = ToDeviceDto(device),
        };

    private static DeviceDto ToDeviceDto(Device device) =>
        new()
        {
            Id = device.Id,
            Name = device.Name,
            ConnectionState = device.ConnectionState,
        };

    private static ChannelDto ToChannelDto(Channel channel, Device device, Board board) =>
        new()
        {
            Id = channel.Id,
            Number = channel.Number,
            State = channel.State,
            LockState = channel.LockState,
            Attributes = channel.Attributes,
            DeviceId = channel.DeviceId,
            BoardId = channel.BoardId,
            Device = ToDeviceDto(device),
            Board = ToBoardDto(board, device),
        };

    private static IRChannelDto ToIRChannelDto(IRChannel irChannel, Board board, Device device) =>
        new()
        {
            Id = irChannel.Id,
            Number = irChannel.Number,
            State = irChannel.State,
            BoardId = irChannel.BoardId,
            Board = ToBoardDto(board, device),
        };

    private static string NormalizeDeviceId(string protocolDeviceId) =>
        protocolDeviceId.Trim();

    private static bool TryMapProtocolLockToLockState(byte raw, out int lockState)
    {
        lockState = LockClosed;
        if (raw == LockiumProtocol.LockDoorOpen)
        {
            lockState = LockOpen;
            return true;
        }

        if (raw == LockiumProtocol.LockDoorClosed)
        {
            lockState = LockClosed;
            return true;
        }

        return false;
    }

    private static bool TryMapLockStateToProtocolStatus(int lockState, out byte lockStatus)
    {
        switch (lockState)
        {
            case LockOpen:
                lockStatus = LockiumProtocol.LockDoorOpen;
                return true;
            case LockClosed:
                lockStatus = LockiumProtocol.LockDoorClosed;
                return true;
            default:
                lockStatus = 0;
                return false;
        }
    }

    private static bool TryParseChannelNumber(string? number, out byte channelNo)
    {
        channelNo = 0;
        if (string.IsNullOrWhiteSpace(number))
            return false;

        if (!byte.TryParse(number.Trim(), out channelNo) || channelNo == 0)
            return false;

        return true;
    }
}
