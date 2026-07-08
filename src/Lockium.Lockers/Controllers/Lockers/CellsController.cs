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

namespace Lockium.Lockers.Controllers.Lockers
{
    /// <summary>
    /// Ячейка шкафа
    /// </summary>
    [Route("/api/v1/cells")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdministrator,Administrator")]
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
        /// Search of Cell using given query
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">List of cells</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/cells/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<CellDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<CellDto>> SearchAsync([FromBody] CellQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Locker).
                Include(_ => _.Channel));
        }

        /// <summary>
        /// Get the cell by id
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Cell data</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/cells/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(CellDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
