using Core.Workflow;
using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Workflows.Reservations.Steps;
using Microsoft.Extensions.Logging;

namespace Lockium.Workflows.Reservations
{
    public class MicroserviceStateWorkflow : MicroserviceStateWorkflowBase, IMicroserviceStateWorkflow
    {
        public MicroserviceStateWorkflow(LockiumDbContext db, ILogger<MicroserviceStateWorkflow> logger)
            : base(db, logger)
        {
        }

        public override async Task<bool> ConfigureMachine(StepContext stepContext)
        {
            var reservationActiveStep = new ReservationActiveStep();
            var reservationCompletedStep = new ReservationCompletedStep();

            AddDefinition(ObjectTypeNames.Reservation, "Active", reservationActiveStep);
            AddDefinition(ObjectTypeNames.Reservation, "Completed", reservationCompletedStep);

            machine.AddTransition((int)ReservationStateIds.Undefined, (int)ReservationStateIds.Active, ObjectTypeNames.Reservation, "Active");
            machine.AddTransition((int)ReservationStateIds.Active, (int)ReservationStateIds.Completed, ObjectTypeNames.Reservation, "Completed");

            return true;
        }
    }
}
