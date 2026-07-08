using Core.Workflow;
using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Workflows.Reservations;

namespace Lockium.Workflows.Reservations.Steps
{
    public abstract class ReservationStepBase<TStepContextInput> : StepBase2<TStepContextInput>
        where TStepContextInput : StepContextInput
    {
        protected LockiumDbContext db = null!;

        public override void SetContext(StepContext stepContext)
        {
            base.SetContext(stepContext);
            db = input.Db;
        }
    }
}
