using Core.Workflow;
using Lockium.Client.Api.Models.Orders;
using Lockium.Client.Api.Services;
using Lockium.Client.Api.Services.Orders;
using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Models.Dtos.Orders;
using Lockium.Workflows.Models;
using Lockium.Workflows.Orders;
using Lockium.Workflows.Orders.Steps;
using Microsoft.EntityFrameworkCore;

namespace Lockium.Client.Api.Services.Orders;

public interface IOrderCreateService
{
    Task<object> CreateAsync(OrderDto request, CancellationToken cancellationToken);
}

public sealed class OrderCreateService(
    LockiumDbContext db,
    IMicroserviceStateWorkflow stateWorkflow) : IOrderCreateService
{
    public async Task<object> CreateAsync(OrderDto request, CancellationToken cancellationToken)
    {
        var stepContext = new StepContext
        {
            ObjectType = ObjectTypeNames.Order,
            StepName = "Created",
            Input = new StepContextInput
            {
                Id = 0,
                PreviousState = (int)OrderStateIds.Undefined,
                NextState = (int)OrderStateIds.Created,
                Db = db,
                Request = request,
            },
        };

        await stateWorkflow.RunAsync(
            (int)OrderStateIds.Undefined,
            (int)OrderStateIds.Created,
            stepContext);

        if (!stepContext.Result.Success)
            return stepContext.Result;

        return stepContext.Result.Data ?? stepContext.Output!;
    }
}
