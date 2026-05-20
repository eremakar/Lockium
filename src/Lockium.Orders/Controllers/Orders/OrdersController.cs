using Data.Repository;
using Data.Repository.Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;
using Lockium.Data.LockiumDb.Entities.Orders;
using Lockium.Models.Dtos.Orders;
using Lockium.Models.Queries.Orders.Orders;
using Lockium.Mappings.Orders;
using Lockium.Data.LockiumDb.DatabaseContext;

namespace Lockium.Orders.Controllers.Orders
{
    [Route("/api/v1/orders")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdministrator,Administrator")]
    public partial class OrdersController : RestControllerBase2<Order, long, OrderDto, OrderQuery, OrderMap>
    {
        public OrdersController(ILogger<RestServiceBase<Order, long>> logger,
            IDapperDbContext restDapperDb,
            LockiumDbContext restDb,
            OrderMap orderMap)
            : base(logger,
                restDapperDb,
                restDb,
                "Orders",
                orderMap)
        {
        }

        /// <summary>
        /// Search of Order using given query
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">List of orders</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/orders/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<OrderDto>> SearchAsync([FromBody] OrderQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Client).
                Include(_ => _.Channel));
        }

        /// <summary>
        /// Get the order by id
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Order data</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/orders/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<OrderDto> FindAsync([FromRoute] long key)
        {
            return await FindUsingEfAsync(key, _ => _.
                Include(_ => _.Client).
                Include(_ => _.Channel));
        }

    }
}
