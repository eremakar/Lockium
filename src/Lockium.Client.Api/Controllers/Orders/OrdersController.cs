using Data.Repository;
using Data.Repository.Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;
using Lockium.Data.LockiumDb.Entities;
using Lockium.Data.LockiumDb.Entities.Devices;
using Lockium.Data.LockiumDb.Entities.Orders;
using Lockium.Models.Dtos.Orders;
using Lockium.Models.Queries.Orders.Orders;
using Lockium.Mappings.Orders;
using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Workflows.Orders;

namespace Lockium.Client.Api.Controllers.Orders
{
    /// <summary>
    /// Заказы размещения в ячейке постамата.
    /// </summary>
    /// <remarks>
    /// Клиентский API для чтения заказов и запуска жизненного цикла через workflow.
    /// Создание — <c>POST /api/v1/orders</c>; смена состояния существующего заказа — <c>POST /api/v1/orders/workflow/run</c>.
    /// Прямое обновление и удаление через REST не поддерживаются.
    /// </remarks>
    [Route("/api/v1/orders")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Client,SuperAdministrator,Administrator")]
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
                ClientId = o.ClientId,
                ChannelId = o.ChannelId,
                Client = o.Client == null
                    ? null
                    : new User
                    {
                        Id = o.Client.Id,
                        UserName = o.Client.UserName,
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
        /// Поиск заказов по фильтру и сортировке.
        /// </summary>
        /// <remarks>
        /// Возвращает постраничный список заказов с вложенными данными клиента, ячейки и устройства.
        /// Тело запроса — объект <see cref="OrderQuery"/> (фильтр, сортировка, пагинация).
        /// Типичные фильтры: идентификатор клиента, ячейки, состояние заказа, диапазон дат создания.
        /// </remarks>
        /// <param name="query">Параметры поиска, фильтрации и постраничной выборки.</param>
        /// <response code="200">Страница заказов.</response>
        /// <response code="400">Ошибка валидации тела запроса.</response>
        /// <response code="401">Не передан или недействителен JWT.</response>
        /// <response code="403">Недостаточно прав (роль не из списка Client / Administrator / SuperAdministrator).</response>
        [Route("/api/v1/orders/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<OrderDto>> SearchAsync([FromBody] OrderQuery query)
        {
            return await SearchUsingEfAsync(query, OrderQuery);
        }

        /// <summary>
        /// Получить заказ по идентификатору.
        /// </summary>
        /// <remarks>
        /// Возвращает один заказ с клиентом, ячейкой (номер, состояние, замок, габариты) и устройством-владельцем ячейки.
        /// </remarks>
        /// <param name="key">Идентификатор заказа.</param>
        /// <response code="200">Данные заказа.</response>
        /// <response code="400">Заказ не найден или некорректный идентификатор.</response>
        /// <response code="401">Не передан или недействителен JWT.</response>
        /// <response code="403">Недостаточно прав.</response>
        [Route("/api/v1/orders/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<OrderDto> FindAsync([FromRoute] long key)
        {
            return await FindUsingEfAsync(key, OrderQuery);
        }

    }
}
