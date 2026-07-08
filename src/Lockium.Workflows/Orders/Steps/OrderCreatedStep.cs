using Core.Workflow;
using Lockium.Data.LockiumDb.Entities.Lockers;
using Lockium.Data.LockiumDb.Entities.Orders;
using Lockium.Data.LockiumDb.Entities.Reservations;
using Lockium.Data.LockiumDb.Entities.Transactions;
using Lockium.Data.LockiumDb.Ids;
using Lockium.Models.Dtos.Orders;
using Lockium.Workflows;
using Lockium.Workflows.Orders;
using Lockium.Workflows.Reservations;
using Lockium.Workflows.Reservations.Steps;
using Microsoft.EntityFrameworkCore;

namespace Lockium.Workflows.Orders.Steps
{
    public partial class OrderCreatedStep : OrderAnyStep
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
                stepContext.Result.Reject("Request is only supported when creating an order");
                return;
            }

            if (input.PreviousState != (int)OrderStateIds.Undefined ||
                input.NextState != (int)OrderStateIds.Created)
            {
                stepContext.Result.Reject(
                    $"Request is only supported for transition {(int)OrderStateIds.Undefined} -> {(int)OrderStateIds.Created}");
                return;
            }

            if (input.Request.Id != 0 &&
                await db.Orders!.AnyAsync(o => o.Id == input.Request.Id))
            {
                stepContext.Result.Reject($"Order with id {input.Request.Id} already exists");
                return;
            }

            var data = MapFromRequest(input.Request);
            data.State = (int)OrderStateIds.Undefined;

            await RunCore(data);

            if (!stepContext.Result.Success)
                return;

            data.State = input.NextState;
            var createdTime = DateTime.UtcNow;
            data.CreatedTime = createdTime;
            data.PinCode ??= GeneratePinCode();

            var transaction = new Transaction
            {
                State = (int)TransactionStateIds.Active,
                SourceType = (int)TransactionSourceTypeIds.Order,
                CreatedTime = createdTime,
                ClientId = data.ClientId,
                Order = data,
            };

            await db.Orders!.AddAsync(data);
            await db.Transactions!.AddAsync(transaction);
            await db.SaveChangesAsync();

            input.Id = data.Id;
            stepContext.Output = data.Id;
            stepContext.Result.Data = new OrderCreatedResult
            {
                OrderId = data.Id,
                ChannelId = data.ChannelId,
                PinCode = data.PinCode,
            };
        }

        private static string GeneratePinCode() =>
            Random.Shared.Next(1000, 10000).ToString();

        private static Order MapFromRequest(OrderDto request)
        {
            var data = new Order
            {
                ClientId = request.ClientId,
                LockerId = request.LockerId,
                ChannelId = request.ChannelId,
                PinCode = request.PinCode,
                TrackingNumber = request.TrackingNumber,
                ExpiresAt = request.ExpiresAt,
            };

            if (request.Id != 0)
                data.Id = request.Id;

            return data;
        }

        public override async Task RunCore(Order data)
        {
            if (!data.ChannelId.HasValue)
            {
                if (!data.LockerId.HasValue)
                {
                    stepContext.Result.Reject("Channel or Locker is required");
                    return;
                }

                var channelId = await ResolveFreeChannelInLockerAsync(data.LockerId.Value);
                if (channelId == null)
                {
                    stepContext.Result.Reject("No free channel found in locker");
                    return;
                }

                data.ChannelId = channelId;
            }

            var channelIdValue = data.ChannelId!.Value;

            var channel = await db.Channels!
                .FirstOrDefaultAsync(c => c.Id == channelIdValue);

            if (channel == null)
            {
                stepContext.Result.Reject($"Channel {channelIdValue} not found");
                return;
            }

            if (channel.State == (int)ChannelStateIds.Occupied)
            {
                stepContext.Result.Reject("Channel is occupied");
                return;
            }

            var hasActiveOrder = await db.Orders!.AnyAsync(o =>
                o.ChannelId == channelIdValue &&
                o.Id != data.Id &&
                (o.State == (int)OrderStateIds.Created || o.State == (int)OrderStateIds.Occupied));

            if (hasActiveOrder)
            {
                stepContext.Result.Reject("Channel is occupied");
                return;
            }

            if (channel.State == (int)ChannelStateIds.Reserved)
            {
                var reservation = await db.Reservations!
                    .Where(r => r.ChannelId == channelIdValue && r.State == (int)ReservationStateIds.Active)
                    .OrderByDescending(r => r.Id)
                    .FirstOrDefaultAsync();

                if (reservation == null)
                {
                    stepContext.Result.Reject("Channel is reserved but no active reservation found");
                    return;
                }

                if (reservation.ClientId != data.ClientId)
                {
                    stepContext.Result.Reject("Channel is occupied");
                    return;
                }

                if (!await CompleteReservationAsync(reservation))
                    return;

                channel = await db.Channels!.FirstOrDefaultAsync(c => c.Id == channelIdValue);
                if (channel == null)
                {
                    stepContext.Result.Reject($"Channel {channelIdValue} not found");
                    return;
                }
            }
            else if (channel.State != (int)ChannelStateIds.Free)
            {
                stepContext.Result.Reject("Channel is occupied");
                return;
            }

            channel.State = (int)ChannelStateIds.Reserved;
            db.Channels!.Update(channel);
        }

        private async Task<long?> ResolveFreeChannelInLockerAsync(long lockerId)
        {
            var cells = await db.Cells!
                .AsNoTracking()
                .Where(c => c.LockerId == lockerId && c.ChannelId != null)
                .Select(c => new { c.ChannelId })
                .ToListAsync();

            foreach (var cell in cells)
            {
                var channelId = cell.ChannelId!.Value;
                var channel = await db.Channels!.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == channelId);

                if (channel?.State != (int)ChannelStateIds.Free)
                    continue;

                var hasActiveOrder = await db.Orders!.AnyAsync(o =>
                    o.ChannelId == channelId &&
                    (o.State == (int)OrderStateIds.Created || o.State == (int)OrderStateIds.Occupied));

                if (!hasActiveOrder)
                    return channelId;
            }

            return null;
        }

        private async Task<bool> CompleteReservationAsync(Reservation reservation)
        {
            var reservationContext = new StepContext
            {
                ObjectType = Reservations.ObjectTypeNames.Reservation,
                StepName = "Completed",
                Input = new Reservations.StepContextInput
                {
                    Id = reservation.Id,
                    PreviousState = (int)Reservations.ReservationStateIds.Active,
                    NextState = (int)Reservations.ReservationStateIds.Completed,
                    Db = db,
                },
            };

            var completedStep = new ReservationCompletedStep();
            completedStep.SetContext(reservationContext);
            await completedStep.Run();

            if (reservationContext.Result.Success)
                return true;

            foreach (var error in reservationContext.Result.Errors)
                stepContext.Result.AddError(error);

            stepContext.Result.State = reservationContext.Result.State;
            return false;
        }
    }

    public sealed class OrderCreatedResult
    {
        public long OrderId { get; set; }
        public long? ChannelId { get; set; }
        public string? PinCode { get; set; }
    }
}
