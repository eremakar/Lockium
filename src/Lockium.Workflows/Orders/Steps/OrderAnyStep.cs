using Core.Workflow;
using Lockium.Data.LockiumDb.Entities.Orders;
using Lockium.Workflows.Orders;
using Microsoft.EntityFrameworkCore;

namespace Lockium.Workflows.Orders.Steps
{
    public partial class OrderAnyStep : OrderStepBase<StepContextInput>
    {
        public override async Task Run()
        {
            var previousState = input.PreviousState;
            var nextState = input.NextState;

            var original = await db.Orders
                .FirstOrDefaultAsync(_ => _.Id == input.Id);

            if (original == null)
            {
                stepContext.Result.Reject($"Order with id {input.Id} not exist in db");
                return;
            }

            if (original.State != previousState)
            {
                stepContext.Result.Reject($"Order with id {input.Id} is not in state {previousState}");
                return;
            }

            await RunCore(original);

            if (!stepContext.Result.Success)
                return;

            original.State = nextState;

            await db.SaveChangesAsync();
        }

        public virtual Task RunCore(Order data) => Task.CompletedTask;
    }
}
