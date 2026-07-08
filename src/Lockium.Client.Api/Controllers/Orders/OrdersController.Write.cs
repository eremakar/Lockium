using Core.Workflow;
using Lockium.Client.Api.Services.Orders;
using Lockium.Models.Dtos.Orders;
using Lockium.Workflows.Orders;
using Lockium.Workflows.Orders.Steps;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Lockium.Client.Api.Controllers.Orders
{
    public partial class OrdersController
    {
        /// <summary>
        /// Создать заказ (переход workflow 0 → 1 «Создан»).
        /// </summary>
        [Route("/api/v1/orders")]
        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StepResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<object> CreateAsync(
            [FromBody] OrderDto request,
            [FromServices] IOrderCreateService orderCreateService)
        {
            return await orderCreateService.CreateAsync(request, HttpContext.RequestAborted);
        }
    }
}
