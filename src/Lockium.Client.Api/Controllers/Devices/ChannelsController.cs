using Data.Repository;
using Data.Repository.Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;
using Lockium.Data.LockiumDb.Entities.Devices;
using Lockium.Models.Dtos.Devices;
using Lockium.Models.Queries.Devices.Channels;
using Lockium.Mappings.Devices;
using Lockium.Data.LockiumDb.DatabaseContext;

namespace Lockium.Client.Api.Controllers.Devices
{
    /// <summary>
    /// Ячейки постамата — чтение состояния и привязки к устройству.
    /// </summary>
    /// <remarks>
    /// Состояние ячейки: `1` — свободна, `2` — забронирована, `3` — занята.
    /// Состояние замка: `1` — закрыт, `2` — открыт.
    /// В атрибуте <c>Attributes</c> могут быть габариты (Width, Height, Length).
    /// </remarks>
    [Route("/api/v1/channels")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Client,SuperAdministrator,Administrator")]
    public partial class ChannelsController : RestControllerBase2<Channel, long, ChannelDto, ChannelQuery, ChannelMap>
    {
        public ChannelsController(ILogger<RestServiceBase<Channel, long>> logger,
            IDapperDbContext restDapperDb,
            LockiumDbContext restDb,
            ChannelMap channelMap)
            : base(logger,
                restDapperDb,
                restDb,
                "Channels",
                channelMap)
        {
        }

        /// <summary>
        /// Поиск ячеек по фильтру.
        /// </summary>
        /// <remarks>
        /// Постраничный список ячеек с вложенным устройством (<c>Device</c>).
        /// Удобно для выбора свободной ячейки перед бронированием или созданием заказа.
        /// </remarks>
        /// <param name="query">Параметры поиска (<see cref="ChannelQuery"/>).</param>
        /// <response code="200">Страница ячеек.</response>
        /// <response code="400">Ошибка валидации запроса.</response>
        /// <response code="401">Не передан или недействителен JWT.</response>
        /// <response code="403">Недостаточно прав.</response>
        [Route("/api/v1/channels/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<ChannelDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<ChannelDto>> SearchAsync([FromBody] ChannelQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Device));
        }

        /// <summary>
        /// Получить ячейку по идентификатору.
        /// </summary>
        /// <remarks>
        /// Возвращает ячейку с данными родительского устройства (имя, состояние подключения).
        /// </remarks>
        /// <param name="key">Идентификатор ячейки.</param>
        /// <response code="200">Данные ячейки.</response>
        /// <response code="400">Ячейка не найдена.</response>
        /// <response code="401">Не передан или недействителен JWT.</response>
        /// <response code="403">Недостаточно прав.</response>
        [Route("/api/v1/channels/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(ChannelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<ChannelDto> FindAsync([FromRoute] long key)
        {
            return await FindUsingEfAsync(key, _ => _.
                Include(_ => _.Device));
        }

    }
}
