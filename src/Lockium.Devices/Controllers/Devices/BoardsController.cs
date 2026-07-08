using Data.Repository;
using Data.Repository.Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;
using Lockium.Data.LockiumDb.Entities.Devices;
using Lockium.Models.Dtos.Devices;
using Lockium.Models.Queries.Devices.Boards;
using Lockium.Mappings.Devices;
using Lockium.Data.LockiumDb.DatabaseContext;

namespace Lockium.Devices.Controllers.Devices
{
    [Route("/api/v1/boards")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdministrator,Administrator")]
    public partial class BoardsController : RestControllerBase2<Board, long, BoardDto, BoardQuery, BoardMap>
    {
        public BoardsController(ILogger<RestServiceBase<Board, long>> logger,
            IDapperDbContext restDapperDb,
            LockiumDbContext restDb,
            BoardMap boardMap)
            : base(logger,
                restDapperDb,
                restDb,
                "Boards",
                boardMap)
        {
        }

        /// <summary>
        /// Search of Board using given query
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">List of boards</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/boards/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<BoardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<BoardDto>> SearchAsync([FromBody] BoardQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Device).
                Include(_ => _.Up).
                Include(_ => _.Channels).
                Include(_ => _.IRs));
        }

        /// <summary>
        /// Get the board by id
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Board data</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/boards/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(BoardDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<BoardDto> FindAsync([FromRoute] long key)
        {
            return await FindUsingEfAsync(key, _ => _.
                Include(_ => _.Device).
                Include(_ => _.Up).
                Include(_ => _.Channels).
                Include(_ => _.IRs));
        }

    }
}
