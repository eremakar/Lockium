using Core.Workflow;
using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Models.Dtos.Reservations;
using Lockium.Workflows.Models;
using Lockium.Workflows.Reservations;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Lockium.Reservations.Controllers.Reservations
{
    public partial class ReservationsController
    {
        private static string? GetStepName(int previousState, int nextState) => (previousState, nextState) switch
        {
            ((int)ReservationStateIds.Undefined, (int)ReservationStateIds.Active) => "Active",
            ((int)ReservationStateIds.Active, (int)ReservationStateIds.Completed) => "Completed",
            _ => null,
        };

        private async Task<StepResult> RunTransitionAsync(
            long id,
            int previousState,
            int nextState,
            ReservationDto? request = null)
        {
            var stepName = GetStepName(previousState, nextState);
            if (stepName == null)
            {
                var result = new StepResult();
                result.Reject($"Transition from {previousState} to {nextState} is not defined");
                return result;
            }

            var db = (LockiumDbContext)restDb;
            var stepContext = new StepContext
            {
                ObjectType = ObjectTypeNames.Reservation,
                StepName = stepName,
                Input = new StepContextInput
                {
                    Id = id,
                    PreviousState = previousState,
                    NextState = nextState,
                    Db = db,
                    Request = request,
                },
            };

            await stateWorkflow.RunAsync(previousState, nextState, stepContext);
            return stepContext.Result;
        }

        /// <summary>
        /// Run reservation state workflow transition
        /// </summary>
        [Route("/api/v1/reservations/workflow/run")]
        [HttpPost]
        [ProducesResponseType(typeof(StepResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StepResult), StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> RunWorkflowAsync([FromBody] WorkflowRunRequest request)
        {
            var result = await RunTransitionAsync(request.Id, request.PreviousState, request.NextState);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

    }
}
