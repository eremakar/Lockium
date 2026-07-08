using Core.Workflow;
using Lockium.Data.LockiumDb.Entities.Billings;
using Lockium.Data.LockiumDb.Entities.Orders;
using Lockium.Data.LockiumDb.Ids;
using Lockium.Workflows;
using Lockium.Workflows.Services;
using Lockium.Workflows.Orders;
using Microsoft.EntityFrameworkCore;

namespace Lockium.Workflows.Orders.Steps
{
    public partial class OrderCompletedStep : OrderAnyStep
    {
        private readonly IBillingCalculator billingCalculator;

        public OrderCompletedStep(IBillingCalculator billingCalculator)
        {
            this.billingCalculator = billingCalculator;
        }

        public override async Task RunCore(Order data)
        {
            if (!data.PickupOpened)
            {
                stepContext.Result.Reject("Pickup open was not performed");
                return;
            }

            if (await db.Billings!.AnyAsync(b => b.OrderId == data.Id))
            {
                stepContext.Result.Reject($"Billing for order {data.Id} already exists");
                return;
            }

            var endTime = DateTime.UtcNow;

            var transaction = await db.Transactions!
                .Where(t => t.OrderId == data.Id)
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();

            if (transaction == null)
            {
                stepContext.Result.Reject($"Transaction for order {data.Id} not found");
                return;
            }

            if (transaction.State == (int)TransactionStateIds.Active)
                transaction.State = (int)TransactionStateIds.Completed;

            var startTime = data.CreatedTime != default
                ? data.CreatedTime
                : transaction.CreatedTime;

            if (data.ChannelId.HasValue)
            {
                var channel = await db.Channels!
                    .FirstOrDefaultAsync(c => c.Id == data.ChannelId.Value);

                if (channel == null)
                    return;

                if (channel.State == (int)ChannelStateIds.Occupied)
                {
                    channel.State = (int)ChannelStateIds.Free;
                    db.Channels!.Update(channel);
                }
            }

            var billing = new Billing
            {
                StartTime = startTime,
                EndTime = endTime,
                Duration = 0,
                Amount = billingCalculator.CalculateAmount(),
                TransactionId = transaction.Id,
                OrderId = data.Id,
            };

            await db.Billings!.AddAsync(billing);
        }
    }
}
