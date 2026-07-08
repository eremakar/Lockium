using Data.Repository;
using Data.Repository.Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;
using Lockium.Data.LockiumDb.Entities.Devices;
using Lockium.Models.Dtos.Devices;
using Lockium.Models.Queries.Devices.IRChannels;
using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Mappings.Devices;

namespace Lockium.Devices.Controllers.Devices
{
    [Route("/api/v1/iRChannels")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdministrator,Administrator")]
    public partial class IRChannelsController : RestControllerBase2<IRChannel, long, IRChannelDto, IRChannelQuery, IRChannelMap>
    {
        public IRChannelsController(ILogger<RestServiceBase<IRChannel, long>> logger,
            IDapperDbContext restDapperDb,
            LockiumDbContext restDb,
            IRChannelMap iRChannelMap)
            : base(logger,
                restDapperDb,
                restDb,
                "IRChannels",
                iRChannelMap)
        {
        }

        /// <summary>
        /// Search of IRChannel using given query
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">List of iRChannels</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/iRChannels/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<IRChannelDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<IRChannelDto>> SearchAsync([FromBody] IRChannelQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Board));
        }

        /// <summary>
        /// Get the iRChannel by id
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">IRChannel data</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/iRChannels/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(IRChannelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<IRChannelDto> FindAsync([FromRoute] long key)
        {
            return await FindUsingEfAsync(key, _ => _.
                Include(_ => _.Board));
        }

    }
}
