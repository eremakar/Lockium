using Core.Workflow;
using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Models.Dtos.Orders;
using Lockium.Workflows.Models;
using Lockium.Workflows.Orders;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Lockium.Client.Api.Controllers.Orders
{
    public partial class OrdersController
    {
        private static string? GetWorkflowStepName(int previousState, int nextState) => (previousState, nextState) switch
        {
            ((int)OrderStateIds.Created, (int)OrderStateIds.Occupied) => "Occupied",
            ((int)OrderStateIds.Occupied, (int)OrderStateIds.Completed) => "Completed",
            _ => null,
        };

        private static string? GetCreateStepName(int previousState, int nextState) => (previousState, nextState) switch
        {
            ((int)OrderStateIds.Undefined, (int)OrderStateIds.Created) => "Created",
            _ => null,
        };

        private async Task<StepResult> RunCreateTransitionAsync(
            long id,
            int previousState,
            int nextState,
            OrderDto request)
        {
            return await RunTransitionCoreAsync(id, previousState, nextState, GetCreateStepName, request);
        }

        private async Task<StepResult> RunTransitionAsync(
            long id,
            int previousState,
            int nextState,
            OrderDto? request = null)
        {
            return await RunTransitionCoreAsync(id, previousState, nextState, GetWorkflowStepName, request);
        }

        private async Task<StepResult> RunTransitionCoreAsync(
            long id,
            int previousState,
            int nextState,
            Func<int, int, string?> getStepName,
            OrderDto? request = null)
        {
            var stepName = getStepName(previousState, nextState);
            if (stepName == null)
            {
                var result = new StepResult();
                result.Reject($"Transition from {previousState} to {nextState} is not defined");
                return result;
            }

            var db = (LockiumDbContext)restDb;
            var stepContext = new StepContext
            {
                ObjectType = ObjectTypeNames.Order,
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
        /// Выполнить переход состояния заказа (workflow).
        /// </summary>
        /// <remarks>
        /// Допустимые переходы в Client API:
        /// - **1 → 2** (Created → Occupied) — заказ занят, клиент разместил вещи;
        /// - **2 → 3** (Occupied → Completed) — завершение: закрывается транзакция, создаётся биллинг, ячейка освобождается.
        ///
        /// В теле передайте <c>Id</c> заказа, <c>PreviousState</c> и <c>NextState</c> (текущее и целевое значения из enum состояний).
        /// Недопустимая пара состояний вернёт 400 с описанием в <c>StepResult</c>.
        /// </remarks>
        /// <param name="request">Идентификатор заказа и пара состояний для перехода.</param>
        /// <response code="200">Переход выполнен успешно.</response>
        /// <response code="400">Переход запрещён или ошибка шага (дубликат биллинга, нет транзакции и т.д.).</response>
        /// <response code="401">Не передан или недействителен JWT.</response>
        /// <response code="403">Недостаточно прав.</response>
        [Route("/api/v1/orders/workflow/run")]
        [HttpPost]
        [ProducesResponseType(typeof(StepResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StepResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
