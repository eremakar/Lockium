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

namespace Lockium.Lockers.Controllers.Lockers
{
    /// <summary>
    /// Шкаф (постамат и т.п.)
    /// </summary>
    [Route("/api/v1/lockers")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdministrator,Administrator")]
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
        /// Search of Locker using given query
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">List of lockers</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/lockers/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<LockerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<LockerDto>> SearchAsync([FromBody] LockerQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Cells));
        }

        /// <summary>
        /// Get the locker by id
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Locker data</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/lockers/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(LockerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<LockerDto> FindAsync([FromRoute] long key)
        {
            return await FindUsingEfAsync(key, _ => _.
                Include(_ => _.Cells));
        }

    }
}
