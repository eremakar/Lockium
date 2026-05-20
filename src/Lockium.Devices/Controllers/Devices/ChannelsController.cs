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

namespace Lockium.Devices.Controllers.Devices
{
    [Route("/api/v1/channels")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdministrator,Administrator")]
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
        /// Search of Channel using given query
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">List of channels</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/channels/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<ChannelDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<ChannelDto>> SearchAsync([FromBody] ChannelQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Device));
        }

        /// <summary>
        /// Get the channel by id
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Channel data</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/channels/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(ChannelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<ChannelDto> FindAsync([FromRoute] long key)
        {
            return await FindUsingEfAsync(key, _ => _.
                Include(_ => _.Device));
        }

    }
}
