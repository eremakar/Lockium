using Data.Repository;
using Data.Repository.Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;
using Lockium.Data.LockiumDb.Entities.Lockers;
using Lockium.Models.Dtos.Lockers;
using Lockium.Models.Queries.Lockers.Lockers;
using Lockium.Mappings.Lockers;
using Lockium.Data.LockiumDb.DatabaseContext;

namespace Lockium.Client.Api.Controllers.Lockers
{
    /// <summary>
    /// Шкафы (постаматы) — чтение каталога для выбора точки размещения.
    /// </summary>
    /// <remarks>
    /// Только операции чтения: поиск и получение по id. Список ячеек шкафа подгружается вместе с шкафом.
    /// Создание, изменение и удаление шкафов через Client API не поддерживаются.
    /// </remarks>
    [Route("/api/v1/lockers")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Client,SuperAdministrator,Administrator")]
    public partial class LockersController : RestControllerBase2<Locker, long, LockerDto, LockerQuery, LockerMap>
    {
        public LockersController(ILogger<RestServiceBase<Locker, long>> logger,
            IDapperDbContext restDapperDb,
            LockiumDbContext restDb,
            LockerMap lockerMap)
            : base(logger,
                restDapperDb,
                restDb,
                "Lockers",
                lockerMap)
        {
        }

        /// <summary>
        /// Поиск шкафов по фильтру.
        /// </summary>
        /// <remarks>
        /// Постраничный список постаматов. Для каждого шкафа в ответе заполняется коллекция <c>Cells</c>.
        /// </remarks>
        /// <param name="query">Параметры поиска (<see cref="LockerQuery"/>).</param>
        /// <response code="200">Страница шкафов с ячейками.</response>
        /// <response code="400">Ошибка валидации запроса.</response>
        /// <response code="401">Не передан или недействителен JWT.</response>
        /// <response code="403">Недостаточно прав.</response>
        [Route("/api/v1/lockers/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<LockerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<LockerDto>> SearchAsync([FromBody] LockerQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Cells));
        }

        /// <summary>
        /// Получить шкаф по идентификатору.
        /// </summary>
        /// <remarks>
        /// Возвращает постамат и полный список его ячеек (номер, состояние, габариты).
        /// </remarks>
        /// <param name="key">Идентификатор шкафа.</param>
        /// <response code="200">Шкаф с ячейками.</response>
        /// <response code="400">Шкаф не найден.</response>
        /// <response code="401">Не передан или недействителен JWT.</response>
        /// <response code="403">Недостаточно прав.</response>
        [Route("/api/v1/lockers/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(LockerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<LockerDto> FindAsync([FromRoute] long key)
        {
            return await FindUsingEfAsync(key, _ => _.
                Include(_ => _.Cells));
        }

    }
}
