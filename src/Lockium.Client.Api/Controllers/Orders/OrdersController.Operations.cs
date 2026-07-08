using Lockium.Client.Api.Models.Orders;
using Lockium.Client.Api.Services;
using Lockium.Client.Api.Services.Orders;
using Lockium.Workflows.Orders.Steps;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Lockium.Client.Api.Controllers.Orders
{
    public partial class OrdersController
    {
        /// <summary>
        /// Открыть ячейку для размещения посылки (один раз, статус = 1 «Создан»).
        /// </summary>
        [Route("/api/v1/orders/{id}/deposit/open")]
        [HttpPost]
        [ProducesResponseType(typeof(OrderLockOpenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OrderLockOpenResponse), StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> OpenForDepositAsync([FromRoute] long id, [FromServices] ILockiumGateway lockiumGateway)
        {
            var result = await lockiumGateway.OpenForDepositAsync(id, HttpContext.RequestAborted);
            if (result is not { Success: true })
                return BadRequest(result ?? new OrderLockOpenResponse { Success = false, Error = "Lock service unavailable", OrderId = id });

            return Ok(result);
        }

        /// <summary>
        /// Подтвердить размещение: дверца закрыта → переход 1 → 2 «Занят».
        /// </summary>
        [Route("/api/v1/orders/{id}/deposited")]
        [HttpPost]
        [ProducesResponseType(typeof(OrderOperationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OrderOperationResult), StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> ConfirmDepositedAsync(
            [FromRoute] long id,
            [FromServices] IOrderOperationsService orderOperations)
        {
            var result = await orderOperations.ConfirmDepositedAsync(id, HttpContext.RequestAborted);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Открыть ячейку для получения посылки (один раз, статус = 2 «Занят»).
        /// </summary>
        [Route("/api/v1/orders/{id}/pickup/open")]
        [HttpPost]
        [ProducesResponseType(typeof(OrderLockOpenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OrderLockOpenResponse), StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> OpenForPickupAsync([FromRoute] long id, [FromServices] ILockiumGateway lockiumGateway)
        {
            var result = await lockiumGateway.OpenForPickupAsync(id, HttpContext.RequestAborted);
            if (result is not { Success: true })
                return BadRequest(result ?? new OrderLockOpenResponse { Success = false, Error = "Lock service unavailable", OrderId = id });

            return Ok(result);
        }

        /// <summary>
        /// Подтвердить получение: дверца закрыта → переход 2 → 3 «Выполнен».
        /// </summary>
        [Route("/api/v1/orders/{id}/pickup")]
        [HttpPost]
        [ProducesResponseType(typeof(OrderOperationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OrderOperationResult), StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> ConfirmPickupAsync(
            [FromRoute] long id,
            [FromServices] IOrderOperationsService orderOperations)
        {
            var result = await orderOperations.ConfirmPickupAsync(id, HttpContext.RequestAborted);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
