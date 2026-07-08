using Core.Workflow;
using Lockium.Data.LockiumDb.Entities.Reservations;
using Lockium.Models.Dtos.Reservations;
using Lockium.Workflows;
using Lockium.Workflows.Reservations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Lockium.Workflows.Reservations.Steps
{
    public partial class ReservationActiveStep : ReservationAnyStep
    {
        public override async Task Run()
        {
            if (input.Request == null)
            {
                await base.Run();
                return;
            }

            if (input.Id != 0)
            {
                stepContext.Result.Reject("Request is only supported when creating a reservation");
                return;
            }

            if (input.PreviousState != (int)ReservationStateIds.Undefined ||
                input.NextState != (int)ReservationStateIds.Active)
            {
                stepContext.Result.Reject(
                    $"Request is only supported for transition {(int)ReservationStateIds.Undefined} -> {(int)ReservationStateIds.Active}");
                return;
            }

            if (input.Request.Id != 0 &&
                await db.Reservations!.AnyAsync(r => r.Id == input.Request.Id))
            {
                stepContext.Result.Reject($"Reservation with id {input.Request.Id} already exists");
                return;
            }

            var data = MapFromRequest(input.Request);
            data.State = (int)ReservationStateIds.Undefined;

            await RunCore(data);

            if (!stepContext.Result.Success)
                return;

            data.State = input.NextState;
            data.CreatedTime = DateTime.UtcNow;
            await db.Reservations!.AddAsync(data);
            await db.SaveChangesAsync();

            input.Id = data.Id;
            stepContext.Output = data.Id;
            stepContext.Result.Data = data.Id;
        }

        private static Reservation MapFromRequest(ReservationDto request)
        {
            var data = new Reservation
            {
                ClientId = request.ClientId,
                CellId = request.CellId,
                ChannelId = request.ChannelId,
                Recipient = request.Recipient != null
                    ? JsonConvert.SerializeObject(request.Recipient)
                    : null,
            };

            if (request.Id != 0)
                data.Id = request.Id;

            return data;
        }

        public override async Task RunCore(Reservation data)
        {
            if (!await ResolveCellAndChannelAsync(data))
                return;

            var channelId = data.ChannelId!.Value;

            var channel = await db.Channels!
                .FirstOrDefaultAsync(c => c.Id == channelId);

            if (channel == null)
            {
                stepContext.Result.Reject($"Channel {channelId} not found");
                return;
            }

            if (channel.State != (int)ChannelStateIds.Free)
            {
                stepContext.Result.Reject("Channel is not free");
                return;
            }

            var hasActiveReservation = await db.Reservations!.AnyAsync(r =>
                r.ChannelId == channelId &&
                r.Id != data.Id &&
                r.State == (int)ReservationStateIds.Active);

            if (hasActiveReservation)
            {
                stepContext.Result.Reject("Channel is not free");
                return;
            }

            channel.State = (int)ChannelStateIds.Reserved;
            db.Channels!.Update(channel);
        }

        private async Task<bool> ResolveCellAndChannelAsync(Reservation data)
        {
            if (data.CellId.HasValue)
            {
                var cell = await db.Cells!
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == data.CellId.Value);

                if (cell == null)
                {
                    stepContext.Result.Reject($"Cell {data.CellId} not found");
                    return false;
                }

                if (!data.ChannelId.HasValue)
                {
                    if (!cell.ChannelId.HasValue)
                    {
                        stepContext.Result.Reject($"Cell {data.CellId} has no channel");
                        return false;
                    }

                    data.ChannelId = cell.ChannelId;
                }

                return true;
            }

            if (data.ChannelId.HasValue)
            {
                var cell = await db.Cells!
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.ChannelId == data.ChannelId.Value);

                if (cell != null)
                    data.CellId = cell.Id;

                return true;
            }

            stepContext.Result.Reject("Cell or Channel is required");
            return false;
        }
    }
}
