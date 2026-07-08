using Core.Workflow;
using Lockium.Client.Api.Models.Orders;
using Lockium.Client.Api.Services;
using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Data.LockiumDb.Entities.Orders;
using Lockium.Workflows;
using Lockium.Workflows.Orders;
using Microsoft.EntityFrameworkCore;

namespace Lockium.Client.Api.Services.Orders;

public interface IOrderOperationsService
{
    Task<OrderOperationResult> ConfirmDepositedAsync(long orderId, CancellationToken cancellationToken);
    Task<OrderOperationResult> ConfirmPickupAsync(long orderId, CancellationToken cancellationToken);
    Task<OrderOperationResult> PickupByPinAsync(string pin, CancellationToken cancellationToken);
}

public sealed class OrderOperationsService(
    LockiumDbContext db,
    ILockiumGateway lockiumGateway,
    IMicroserviceStateWorkflow stateWorkflow) : IOrderOperationsService
{
    public async Task<OrderOperationResult> ConfirmDepositedAsync(long orderId, CancellationToken cancellationToken)
    {
        var order = await LoadOrderAsync(orderId, cancellationToken);
        if (order == null)
            return Fail(orderId, $"Order {orderId} not found");

        if (order.State != (int)OrderStateIds.Created)
            return Fail(orderId, "Order must be in Created state");

        if (!order.DepositOpened)
            return Fail(orderId, "Deposit open was not performed");

        var channel = order.Channel;
        if (channel == null)
            return Fail(orderId, "Channel not found");

        if (channel.LockState != (int)ChannelLockStateIds.Closed)
            return Fail(orderId, "Channel door must be closed");

        var workflowResult = await RunTransitionAsync(
            orderId,
            (int)OrderStateIds.Created,
            (int)OrderStateIds.Occupied,
            cancellationToken);

        if (!workflowResult.Success)
            return Fail(orderId, string.Join("; ", workflowResult.Errors));

        return Success(orderId, (int)OrderStateIds.Occupied);
    }

    public async Task<OrderOperationResult> ConfirmPickupAsync(long orderId, CancellationToken cancellationToken)
    {
        var order = await LoadOrderAsync(orderId, cancellationToken);
        if (order == null)
            return Fail(orderId, $"Order {orderId} not found");

        if (order.State != (int)OrderStateIds.Occupied)
            return Fail(orderId, "Order must be in Occupied state");

        if (!order.PickupOpened)
            return Fail(orderId, "Pickup open was not performed");

        var channel = order.Channel;
        if (channel == null)
            return Fail(orderId, "Channel not found");

        if (channel.LockState != (int)ChannelLockStateIds.Closed)
            return Fail(orderId, "Channel door must be closed");

        var workflowResult = await RunTransitionAsync(
            orderId,
            (int)OrderStateIds.Occupied,
            (int)OrderStateIds.Completed,
            cancellationToken);

        if (!workflowResult.Success)
            return Fail(orderId, string.Join("; ", workflowResult.Errors));

        return Success(orderId, (int)OrderStateIds.Completed);
    }

    public async Task<OrderOperationResult> PickupByPinAsync(string pin, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(pin))
            return Fail(0, "Pin is required");

        var order = await db.Orders!
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.PinCode == pin && o.State == (int)OrderStateIds.Occupied, cancellationToken);

        if (order == null)
            return Fail(0, "Active order not found for pin");

        var openResult = await lockiumGateway.OpenForPickupAsync(order.Id, cancellationToken);
        if (openResult is not { Success: true })
            return Fail(order.Id, openResult?.Error ?? "Failed to open cell");

        return new OrderOperationResult
        {
            Success = true,
            OrderId = order.Id,
            State = order.State,
        };
    }

    private async Task<Order?> LoadOrderAsync(long orderId, CancellationToken cancellationToken) =>
        await db.Orders!
            .Include(o => o.Channel)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

    private async Task<StepResult> RunTransitionAsync(
        long id,
        int previousState,
        int nextState,
        CancellationToken cancellationToken)
    {
        var stepName = (previousState, nextState) switch
        {
            ((int)OrderStateIds.Created, (int)OrderStateIds.Occupied) => "Occupied",
            ((int)OrderStateIds.Occupied, (int)OrderStateIds.Completed) => "Completed",
            _ => null,
        };

        if (stepName == null)
        {
            var invalid = new StepResult();
            invalid.Reject($"Transition from {previousState} to {nextState} is not defined");
            return invalid;
        }

        var stepContext = new StepContext
        {
            ObjectType = ObjectTypeNames.Order,
            StepName = stepName,
            Input = new StepContextInput
            {
                Id = id,
                PreviousState = previousState,
                NextState = nextState,
                Db = db,
            },
        };

        await stateWorkflow.RunAsync(previousState, nextState, stepContext);
        return stepContext.Result;
    }

    private static OrderOperationResult Fail(long orderId, string error) =>
        new() { Success = false, Error = error, OrderId = orderId };

    private static OrderOperationResult Success(long orderId, int state) =>
        new() { Success = true, OrderId = orderId, State = state };
}
