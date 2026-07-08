using Lockium.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lockium.Controllers;

[ApiController]
[Route("api/v1/orders")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Client,SuperAdministrator,Administrator")]
public sealed class OrderLockController(IOrderLockService orderLockService) : ControllerBase
{
    /// <summary>
    /// Открыть ячейку для размещения посылки (один раз, статус заказа = 1 «Создан»).
    /// </summary>
    [HttpPost("{orderId}/deposit/open")]
    [ProducesResponseType(typeof(OrderLockOpenResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OrderLockOpenResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderLockOpenResult>> OpenForDepositAsync(
        long orderId,
        CancellationToken cancellationToken)
    {
        var result = await orderLockService.OpenForDepositAsync(orderId, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Открыть ячейку для получения посылки (один раз, статус заказа = 2 «Занят»).
    /// </summary>
    [HttpPost("{orderId}/pickup/open")]
    [ProducesResponseType(typeof(OrderLockOpenResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OrderLockOpenResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderLockOpenResult>> OpenForPickupAsync(
        long orderId,
        CancellationToken cancellationToken)
    {
        var result = await orderLockService.OpenForPickupAsync(orderId, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
