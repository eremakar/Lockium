using Core.Workflow;
using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Workflows.Services;
using Lockium.Workflows.Orders.Steps;
using Microsoft.Extensions.Logging;

namespace Lockium.Workflows.Orders
{
    public class MicroserviceStateWorkflow : MicroserviceStateWorkflowBase, IMicroserviceStateWorkflow
    {
        private readonly IBillingCalculator billingCalculator;

        public MicroserviceStateWorkflow(
            LockiumDbContext db,
            ILogger<MicroserviceStateWorkflow> logger,
            IBillingCalculator billingCalculator)
            : base(db, logger)
        {
            this.billingCalculator = billingCalculator;
        }

        public override async Task<bool> ConfigureMachine(StepContext stepContext)
        {
            var orderCreatedStep = new OrderCreatedStep();
            var orderOccupiedStep = new OrderOccupiedStep();
            var orderCompletedStep = new OrderCompletedStep(billingCalculator);

            AddDefinition(ObjectTypeNames.Order, "Created", orderCreatedStep);
            AddDefinition(ObjectTypeNames.Order, "Occupied", orderOccupiedStep);
            AddDefinition(ObjectTypeNames.Order, "Completed", orderCompletedStep);

            machine.AddTransition((int)OrderStateIds.Undefined, (int)OrderStateIds.Created, ObjectTypeNames.Order, "Created");
            machine.AddTransition((int)OrderStateIds.Created, (int)OrderStateIds.Occupied, ObjectTypeNames.Order, "Occupied");
            machine.AddTransition((int)OrderStateIds.Occupied, (int)OrderStateIds.Completed, ObjectTypeNames.Order, "Completed");

            return true;
        }
    }
}
