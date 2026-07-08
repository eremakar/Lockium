using Core.Workflow;
using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Models.Dtos.Reservations;
using Lockium.Workflows.Reservations;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Lockium.Client.Api.Controllers.Reservations
{
    public partial class ReservationsController
    {
        private static string? GetStepName(int previousState, int nextState) => (previousState, nextState) switch
        {
            ((int)ReservationStateIds.Undefined, (int)ReservationStateIds.Active) => "Active",
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
        /// Забронировать ячейку (переход workflow 0 → 1 «Активна»).
        /// </summary>
        /// <remarks>
        /// Создаёт бронь на указанную ячейку для клиента. Обязательны <c>ChannelId</c> и <c>ClientId</c>.
        ///
        /// Условия:
        /// - ячейка существует и в статусе «свободна»;
        /// - нет другой активной брони на эту ячейку.
        ///
        /// При успехе ячейка переводится в статус «забронирована», в ответе — id брони.
        /// При ошибке — <c>StepResult</c> с HTTP 400.
        /// </remarks>
        /// <param name="request">Клиент и ячейка для брони; <c>Id</c> = 0 при создании.</param>
        /// <response code="200">Бронь создана.</response>
        /// <response code="400">Ячейка занята, не найдена или не свободна.</response>
        /// <response code="401">Не передан или недействителен JWT.</response>
        /// <response code="403">Недостаточно прав.</response>
        [Route("/api/v1/reservations")]
        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StepResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<object> CreateAsync([FromBody] ReservationDto request)
        {
            var workflowResult = await RunTransitionAsync(
                0,
                (int)ReservationStateIds.Undefined,
                (int)ReservationStateIds.Active,
                request);

            if (!workflowResult.Success)
                return workflowResult;

            return workflowResult.Data!;
        }
    }
}
