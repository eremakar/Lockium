using Data.Repository;
using Data.Repository.Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;
using Lockium.Data.LockiumDb.Entities;
using Lockium.Data.LockiumDb.Entities.Devices;
using Lockium.Data.LockiumDb.Entities.Lockers;
using Lockium.Data.LockiumDb.Entities.Orders;
using Lockium.Models.Dtos.Orders;
using Lockium.Models.Queries.Orders.Orders;
using Lockium.Mappings.Orders;
using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Workflows.Orders;

namespace Lockium.Orders.Controllers.Orders
{
    [Route("/api/v1/orders")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdministrator,Administrator")]
    public partial class OrdersController : RestControllerBase2<Order, long, OrderDto, OrderQuery, OrderMap>
    {
        private readonly IMicroserviceStateWorkflow stateWorkflow;

        public OrdersController(ILogger<RestServiceBase<Order, long>> logger,
            IDapperDbContext restDapperDb,
            LockiumDbContext restDb,
            OrderMap orderMap,
            IMicroserviceStateWorkflow stateWorkflow)
            : base(logger,
                restDapperDb,
                restDb,
                "Orders",
                orderMap)
        {
            this.stateWorkflow = stateWorkflow;
        }

        private static IQueryable<Order> OrderQuery(IQueryable<Order> query) =>
            query.Select(o => new Order
            {
                Id = o.Id,
                State = o.State,
                CreatedTime = o.CreatedTime,
                PinCode = o.PinCode,
                DepositOpened = o.DepositOpened,
                PickupOpened = o.PickupOpened,
                TrackingNumber = o.TrackingNumber,
                ExpiresAt = o.ExpiresAt,
                Recipient = o.Recipient,
                ClientId = o.ClientId,
                LockerId = o.LockerId,
                CellId = o.CellId,
                ChannelId = o.ChannelId,
                Client = o.Client == null
                    ? null
                    : new User
                    {
                        Id = o.Client.Id,
                        UserName = o.Client.UserName,
                    },
                Locker = o.Locker == null
                    ? null
                    : new Locker
                    {
                        Id = o.Locker.Id,
                        Name = o.Locker.Name,
                        Address = o.Locker.Address,
                        Type = o.Locker.Type,
                    },
                Cell = o.Cell == null
                    ? null
                    : new Cell
                    {
                        Id = o.Cell.Id,
                        Number = o.Cell.Number,
                        State = o.Cell.State,
                        Attributes = o.Cell.Attributes,
                        LockerId = o.Cell.LockerId,
                        ChannelId = o.Cell.ChannelId,
                    },
                Channel = o.Channel == null
                    ? null
                    : new Channel
                    {
                        Id = o.Channel.Id,
                        Number = o.Channel.Number,
                        State = o.Channel.State,
                        LockState = o.Channel.LockState,
                        Attributes = o.Channel.Attributes,
                        DeviceId = o.Channel.DeviceId,
                        Device = o.Channel.Device == null
                            ? null
                            : new Device
                            {
                                Id = o.Channel.Device.Id,
                                Name = o.Channel.Device.Name,
                                ConnectionState = o.Channel.Device.ConnectionState,
                            },
                    },
            });

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
            return await SearchUsingEfAsync(query, OrderQuery);
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
            return await FindUsingEfAsync(key, OrderQuery);
        }

    }
}
