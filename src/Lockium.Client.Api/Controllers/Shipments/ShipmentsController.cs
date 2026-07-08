using Lockium.Client.Api.Models.Orders;
using Lockium.Client.Api.Services;
using Lockium.Client.Api.Services.Orders;
using Lockium.Models.Dtos.Orders;
using Lockium.Workflows.Orders.Steps;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Lockium.Client.Api.Controllers.Shipments;

/// <summary>
/// Доставки (алиас заказов размещения).
/// </summary>
[Route("/api/v1/shipments")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Client,SuperAdministrator,Administrator")]
public sealed class ShipmentsController(
    IOrderCreateService orderCreateService,
    ILockiumGateway lockiumGateway,
    IOrderOperationsService orderOperations) : ControllerBase
{
    /// <summary>
    /// Создать доставку: подбирается свободная ячейка в шкафу, возвращается PIN.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ShipmentCreateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> CreateAsync([FromBody] ShipmentCreateRequest request)
    {
        var orderRequest = new OrderDto
        {
            ClientId = request.ClientId,
            LockerId = request.LockerId,
            ChannelId = request.ChannelId,
            TrackingNumber = request.TrackingNumber,
            ExpiresAt = request.ExpiresAt ?? default,
        };

        var result = await orderCreateService.CreateAsync(orderRequest, HttpContext.RequestAborted);
        if (result is OrderCreatedResult created)
        {
            return Ok(new ShipmentCreateResponse
            {
                OrderId = created.OrderId,
                CellId = created.ChannelId,
                PinCode = created.PinCode,
            });
        }

        return BadRequest(result);
    }

    [HttpPost("{id}/deposit/open")]
    public async Task<IActionResult> OpenForDepositAsync(long id)
    {
        var result = await lockiumGateway.OpenForDepositAsync(id, HttpContext.RequestAborted);
        if (result is not { Success: true })
            return BadRequest(result ?? new OrderLockOpenResponse { Success = false, Error = "Lock service unavailable", OrderId = id });

        return Ok(result);
    }

    [HttpPost("{id}/deposited")]
    public async Task<IActionResult> ConfirmDepositedAsync(long id)
    {
        var result = await orderOperations.ConfirmDepositedAsync(id, HttpContext.RequestAborted);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/pickup/open")]
    public async Task<IActionResult> OpenForPickupAsync(long id)
    {
        var result = await lockiumGateway.OpenForPickupAsync(id, HttpContext.RequestAborted);
        if (result is not { Success: true })
            return BadRequest(result ?? new OrderLockOpenResponse { Success = false, Error = "Lock service unavailable", OrderId = id });

        return Ok(result);
    }

    [HttpPost("{id}/pickup")]
    public async Task<IActionResult> ConfirmPickupAsync(long id)
    {
        var result = await orderOperations.ConfirmPickupAsync(id, HttpContext.RequestAborted);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
