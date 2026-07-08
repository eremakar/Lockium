using Lockium.Client.Api.Models.Orders;
using Lockium.Client.Api.Services.Orders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Lockium.Client.Api.Controllers.Pickup;

/// <summary>
/// Получение посылки по PIN.
/// </summary>
[Route("/api/v1/pickup")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Client,SuperAdministrator,Administrator")]
public sealed class PickupController(IOrderOperationsService orderOperations) : ControllerBase
{
    /// <summary>
    /// Открыть ячейку по PIN (статус заказа = 2). После закрытия дверцы — <c>POST /api/v1/orders/{id}/pickup</c>.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderOperationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OrderOperationResult), StatusCodes.Status400BadRequest)]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> PickupByPinAsync([FromBody] PickupRequest request)
    {
        var result = await orderOperations.PickupByPinAsync(request.Pin, HttpContext.RequestAborted);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
