using Core.Workflow;
using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Workflows.Orders;

namespace Lockium.Workflows.Orders.Steps
{
    public abstract class OrderStepBase<TStepContextInput> : StepBase2<TStepContextInput>
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
