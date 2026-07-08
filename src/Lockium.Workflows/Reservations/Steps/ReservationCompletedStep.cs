using Core.Workflow;
using Lockium.Data.LockiumDb.Entities.Reservations;
using Lockium.Workflows;
using Lockium.Workflows.Reservations;
using Microsoft.EntityFrameworkCore;

namespace Lockium.Workflows.Reservations.Steps
{
    public partial class ReservationCompletedStep : ReservationAnyStep
    {
        public override async Task RunCore(Reservation data)
        {
            if (!data.ChannelId.HasValue)
                return;

            var channel = await db.Channels!
                .FirstOrDefaultAsync(c => c.Id == data.ChannelId.Value);

            if (channel == null)
                return;

            if (channel.State == (int)ChannelStateIds.Reserved)
            {
                channel.State = (int)ChannelStateIds.Free;
                db.Channels!.Update(channel);
            }
        }
    }
}
