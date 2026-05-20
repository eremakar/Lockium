using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Data.LockiumDb.Entities.Devices;
using Lockium.Models;
using Microsoft.EntityFrameworkCore;

namespace Lockium.Services;

public sealed class DeviceService(LockiumDbContext db) : IDeviceService
{
    /// <summary>Статус подключения в БД: 1 — выключен, 2 — включен, 3 — ошибка.</summary>
    public const int ConnectionOff = 1;

    public const int ConnectionOn = 2;

    /// <summary>Статус замка в Channels: 1 — закрыт, 2 — открыт.</summary>
    public const int LockClosed = 1;

    public const int LockOpen = 2;

    /// <summary>Состояние ячейки по умолчанию при создании: 1 — свободна.</summary>
    public const int ChannelStateFree = 1;

    public async Task UpsertDeviceConnectedAsync(string protocolDeviceId, CancellationToken cancellationToken)
    {
        var key = NormalizeDeviceId(protocolDeviceId);
        if (key.Length == 0)
            return;

        var device = await db.Devices!.AsQueryable().FirstOrDefaultAsync(d => d.Name == key, cancellationToken);
        if (device is null)
        {
            db.Devices!.Add(new Device
            {
                Name = key,
                ConnectionState = ConnectionOn,
            });
        }
        else
            device.ConnectionState = ConnectionOn;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkDeviceDisconnectedAsync(string protocolDeviceId, CancellationToken cancellationToken)
    {
        var key = NormalizeDeviceId(protocolDeviceId);
        if (key.Length == 0)
            return;

        var device = await db.Devices!.AsQueryable().FirstOrDefaultAsync(d => d.Name == key, cancellationToken);
        if (device is null)
            return;

        device.ConnectionState = ConnectionOff;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SyncChannelsFromReadAllAsync(
        string protocolDeviceId,
        AllLockStatusResult result,
        CancellationToken cancellationToken)
    {
        if (result.Status != LockiumProtocol.StatusOk)
            return;

        var key = NormalizeDeviceId(protocolDeviceId);
        if (key.Length == 0)
            return;

        var device = await db.Devices!
            .Include(d => d.Channels)
            .FirstOrDefaultAsync(d => d.Name == key, cancellationToken);

        if (device is null)
        {
            device = new Device
            {
                Name = key,
                ConnectionState = ConnectionOn,
                Channels = [],
            };
            db.Devices!.Add(device);
            await db.SaveChangesAsync(cancellationToken);
        }

        device.Channels ??= [];
        int count = Math.Min(result.ChannelCount, result.LockStatuses.Count);

        for (int i = 0; i < count; i++)
        {
            byte raw = result.LockStatuses[i];
            if (!TryMapProtocolLockToLockState(raw, out var lockState))
                continue;

            string number = ((byte)(i + 1)).ToString();
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

        await db.SaveChangesAsync(cancellationToken);
    }

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

        // LockOutOfBounds / неизвестное — не перетираем данные из ответа
        return false;
    }
}
