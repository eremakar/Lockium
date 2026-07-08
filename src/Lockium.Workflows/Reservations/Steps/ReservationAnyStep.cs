using Core.Workflow;
using Lockium.Data.LockiumDb.Entities.Reservations;
using Lockium.Workflows.Reservations;
using Microsoft.EntityFrameworkCore;

namespace Lockium.Workflows.Reservations.Steps
{
    public partial class ReservationAnyStep : ReservationStepBase<StepContextInput>
    {
        public override async Task Run()
        {
            var previousState = input.PreviousState;
            var nextState = input.NextState;

            var original = await db.Reservations
                .FirstOrDefaultAsync(_ => _.Id == input.Id);

            if (original == null)
            {
                stepContext.Result.Reject($"Reservation with id {input.Id} not exist in db");
                return;
            }

            if (original.State != previousState)
            {
                stepContext.Result.Reject($"Reservation with id {input.Id} is not in state {previousState}");
                return;
            }

            await RunCore(original);

            if (!stepContext.Result.Success)
                return;

            original.State = nextState;

            await db.SaveChangesAsync();
        }

        public virtual Task RunCore(Reservation data) => Task.CompletedTask;
    }
}
