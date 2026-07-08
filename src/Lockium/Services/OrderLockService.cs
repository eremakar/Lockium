using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Data.LockiumDb.Entities.Devices;
using Lockium.Data.LockiumDb.Entities.Orders;
using Lockium.Models;
using Lockium.Workflows;
using Lockium.Workflows.Orders;
using Microsoft.EntityFrameworkCore;

namespace Lockium.Services;

public interface IOrderLockService
{
    Task<OrderLockOpenResult> OpenForDepositAsync(long orderId, CancellationToken cancellationToken);
    Task<OrderLockOpenResult> OpenForPickupAsync(long orderId, CancellationToken cancellationToken);
}

public sealed record OrderLockOpenResult(
    bool Success,
    string? Error,
    long OrderId,
    long? ChannelId,
    OpenLockResult? LockResult = null);

public sealed class OrderLockService(
    LockiumDbContext db,
    LockConnectionRegistry registry,
    IDeviceService deviceService) : IOrderLockService
{
    public Task<OrderLockOpenResult> OpenForDepositAsync(long orderId, CancellationToken cancellationToken) =>
        OpenAsync(
            orderId,
            requiredState: (int)OrderStateIds.Created,
            alreadyOpened: o => o.DepositOpened,
            markOpened: o => o.DepositOpened = true,
            cancellationToken);

    public Task<OrderLockOpenResult> OpenForPickupAsync(long orderId, CancellationToken cancellationToken) =>
        OpenAsync(
            orderId,
            requiredState: (int)OrderStateIds.Occupied,
            alreadyOpened: o => o.PickupOpened,
            markOpened: o => o.PickupOpened = true,
            cancellationToken);

    private async Task<OrderLockOpenResult> OpenAsync(
        long orderId,
        int requiredState,
        Func<Order, bool> alreadyOpened,
        Action<Order> markOpened,
        CancellationToken cancellationToken)
    {
        var order = await db.Orders!
            .Include(o => o.Channel)!.ThenInclude(c => c!.Device)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
            return Fail(orderId, $"Order {orderId} not found");

        if (order.State != requiredState)
            return Fail(orderId, $"Order {orderId} must be in state {requiredState}");

        if (alreadyOpened(order))
            return Fail(orderId, "Cell open already used for this order");

        var channel = order.Channel;
        if (channel?.Device?.Name == null)
            return Fail(orderId, "Order channel or device is not configured");

        if (!TryParseChannelNumber(channel.Number, out var channelNumber))
            return Fail(orderId, $"Invalid channel number '{channel.Number}'");

        var session = registry.Get(channel.Device.Name);
        if (session is null)
            return Fail(orderId, $"Device '{channel.Device.Name}' is not connected");

        var boardNumber = (byte)(channel.BoardId ?? 0);
        var openResult = await session.OpenSingleChannelLockAsync(
            boardNumber,
            channelNumber,
            order.Id.ToString(),
            cancellationToken);

        openResult = await deviceService.EnrichOpenLockAsync(
            channel.Device.Name,
            boardNumber,
            channelNumber,
            openResult,
            cancellationToken);

        markOpened(order);
        channel.LockState = (int)ChannelLockStateIds.Open;
        db.Orders!.Update(order);
        db.Channels!.Update(channel);
        await db.SaveChangesAsync(cancellationToken);

        return new OrderLockOpenResult(true, null, orderId, channel.Id, openResult);
    }

    private static OrderLockOpenResult Fail(long orderId, string error) =>
        new(false, error, orderId, null);

    private static bool TryParseChannelNumber(string? number, out byte channelNumber)
    {
        channelNumber = 0;
        return !string.IsNullOrWhiteSpace(number) && byte.TryParse(number, out channelNumber);
    }
}
