using Data.Repository;
using Data.Repository.Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;
using Lockium.Data.LockiumDb.Entities.Lockers;
using Lockium.Models.Dtos.Lockers;
using Lockium.Models.Queries.Lockers.Cells;
using Lockium.Mappings.Lockers;
using Lockium.Data.LockiumDb.DatabaseContext;

namespace Lockium.Client.Api.Controllers.Lockers
{
    /// <summary>
    /// Ячейки шкафа — чтение состояния и привязки к шкафу и каналу замка.
    /// </summary>
    /// <remarks>
    /// Состояние ячейки: `1` — свободна, `2` — забронирована, `3` — занята.
    /// В атрибуте <c>Attributes</c> могут быть габариты (Width, Height, Length).
    /// </remarks>
    [Route("/api/v1/cells")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Client,SuperAdministrator,Administrator")]
    public partial class CellsController : RestControllerBase2<Cell, long, CellDto, CellQuery, CellMap>
    {
        public CellsController(ILogger<RestServiceBase<Cell, long>> logger,
            IDapperDbContext restDapperDb,
            LockiumDbContext restDb,
            CellMap cellMap)
            : base(logger,
                restDapperDb,
                restDb,
                "Cells",
                cellMap)
        {
        }

        /// <summary>
        /// Поиск ячеек шкафа по фильтру.
        /// </summary>
        /// <remarks>
        /// Постраничный список ячеек с вложенным шкафом (<c>Locker</c>) и каналом замка (<c>Channel</c>).
        /// Удобно для выбора свободной ячейки перед бронированием или созданием заказа.
        /// </remarks>
        /// <param name="query">Параметры поиска (<see cref="CellQuery"/>).</param>
        /// <response code="200">Страница ячеек.</response>
        /// <response code="400">Ошибка валидации запроса.</response>
        /// <response code="401">Не передан или недействителен JWT.</response>
        /// <response code="403">Недостаточно прав.</response>
        [Route("/api/v1/cells/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<CellDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<CellDto>> SearchAsync([FromBody] CellQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Locker).
                Include(_ => _.Channel));
        }

        /// <summary>
        /// Получить ячейку шкафа по идентификатору.
        /// </summary>
        /// <remarks>
        /// Возвращает ячейку с данными шкафа и канала замка.
        /// </remarks>
        /// <param name="key">Идентификатор ячейки.</param>
        /// <response code="200">Данные ячейки.</response>
        /// <response code="400">Ячейка не найдена.</response>
        /// <response code="401">Не передан или недействителен JWT.</response>
        /// <response code="403">Недостаточно прав.</response>
        [Route("/api/v1/cells/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(CellDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<CellDto> FindAsync([FromRoute] long key)
        {
            return await FindUsingEfAsync(key, _ => _.
                Include(_ => _.Locker).
                Include(_ => _.Channel));
        }

    }
}
