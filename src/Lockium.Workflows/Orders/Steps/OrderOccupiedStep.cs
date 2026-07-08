using Lockium.Data.LockiumDb.Entities.Orders;
using Lockium.Workflows;
using Lockium.Workflows.Orders;
using Microsoft.EntityFrameworkCore;

namespace Lockium.Workflows.Orders.Steps
{
    public partial class OrderOccupiedStep : OrderAnyStep
    {
        public override async Task RunCore(Order data)
        {
            if (!data.DepositOpened)
            {
                stepContext.Result.Reject("Deposit open was not performed");
                return;
            }

            if (!data.ChannelId.HasValue)
            {
                stepContext.Result.Reject("Channel is required");
                return;
            }

            var channel = await db.Channels!
                .FirstOrDefaultAsync(c => c.Id == data.ChannelId.Value);

            if (channel == null)
            {
                stepContext.Result.Reject($"Channel {data.ChannelId} not found");
                return;
            }

            if (channel.LockState != (int)ChannelLockStateIds.Closed)
            {
                stepContext.Result.Reject("Channel door must be closed before occupying");
                return;
            }

            if (channel.State == (int)ChannelStateIds.Reserved)
            {
                channel.State = (int)ChannelStateIds.Occupied;
                db.Channels!.Update(channel);
            }
            else if (channel.State != (int)ChannelStateIds.Occupied)
            {
                stepContext.Result.Reject("Channel is not reserved for this order");
            }
        }
    }
}
