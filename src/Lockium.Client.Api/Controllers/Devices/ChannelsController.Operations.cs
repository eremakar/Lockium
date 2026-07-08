using Lockium.Client.Api.Models.Orders;
using Lockium.Client.Api.Services;
using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Workflows.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;

namespace Lockium.Client.Api.Controllers.Devices
{
    public partial class ChannelsController
    {
        /// <summary>
        /// Открыть ячейку по активному заказу (размещение или получение в зависимости от статуса).
        /// </summary>
        [Route("/api/v1/channels/{id}/open")]
        [HttpPost]
        [ProducesResponseType(typeof(OrderLockOpenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OrderLockOpenResponse), StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> OpenByChannelAsync(
            [FromRoute] long id,
            [FromServices] LockiumDbContext db,
            [FromServices] ILockiumGateway lockiumGateway)
        {
            var order = await db.Orders!
                .AsNoTracking()
                .Where(o => o.ChannelId == id &&
                    (o.State == (int)OrderStateIds.Created || o.State == (int)OrderStateIds.Occupied))
                .OrderByDescending(o => o.Id)
                .FirstOrDefaultAsync(HttpContext.RequestAborted);

            if (order == null)
            {
                return BadRequest(new OrderLockOpenResponse
                {
                    Success = false,
                    Error = "No active order for channel",
                    OrderId = 0,
                    ChannelId = id,
                });
            }

            var result = order.State == (int)OrderStateIds.Created
                ? await lockiumGateway.OpenForDepositAsync(order.Id, HttpContext.RequestAborted)
                : await lockiumGateway.OpenForPickupAsync(order.Id, HttpContext.RequestAborted);

            if (result is not { Success: true })
                return BadRequest(result ?? new OrderLockOpenResponse { Success = false, Error = "Lock service unavailable", OrderId = order.Id, ChannelId = id });

            return Ok(result);
        }
    }
}
