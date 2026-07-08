using Core.Workflow;
using Data.Repository;
using Lockium.Data.LockiumDb.DatabaseContext;
using Microsoft.Extensions.Logging;

namespace Lockium.Workflows
{
    public abstract class MicroserviceStateWorkflowBase : StateWorkflowBase<LockiumDbContext, int>
    {
        protected MicroserviceStateWorkflowBase(LockiumDbContext db, ILogger logger)
            : base(db, logger)
        {
        }

        protected void AddDefinition(string objectType, string name, StepBase<StepContext> step)
        {
            machine.AddDefinition(new StepDefinition
            {
                ObjectType = objectType,
                Name = name,
                Step = step,
            });
        }
    }
}
