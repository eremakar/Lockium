using Data.Repository;
using Data.Repository.Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;
using Lockium.Data.LockiumDb.Entities.Devices;
using Lockium.Models.Dtos.Devices;
using Lockium.Models.Queries.Devices.DeviceLogs;
using Lockium.Mappings.Devices;
using Lockium.Data.LockiumDb.DatabaseContext;

namespace Lockium.Devices.Controllers.Devices
{
    /// <summary>
    /// Лог команд и событий устройства
    /// </summary>
    [Route("/api/v1/deviceLogs")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdministrator,Administrator")]
    public partial class DeviceLogsController : RestControllerBase2<DeviceLog, long, DeviceLogDto, DeviceLogQuery, DeviceLogMap>
    {
        public DeviceLogsController(ILogger<RestServiceBase<DeviceLog, long>> logger,
            IDapperDbContext restDapperDb,
            LockiumDbContext restDb,
            DeviceLogMap deviceLogMap)
            : base(logger,
                restDapperDb,
                restDb,
                "DeviceLogs",
                deviceLogMap)
        {
        }

        /// <summary>
        /// Search of DeviceLog using given query
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">List of deviceLogs</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/deviceLogs/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<DeviceLogDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<DeviceLogDto>> SearchAsync([FromBody] DeviceLogQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Device).
                Include(_ => _.Channel));
        }

        /// <summary>
        /// Get the deviceLog by id
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">DeviceLog data</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/deviceLogs/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(DeviceLogDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<DeviceLogDto> FindAsync([FromRoute] long key)
        {
            return await FindUsingEfAsync(key, _ => _.
                Include(_ => _.Device).
                Include(_ => _.Channel));
        }

    }
}
