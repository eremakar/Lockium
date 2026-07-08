using Data.Repository;
using Data.Repository.Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;
using Lockium.Data.LockiumDb.Entities.Devices;
using Lockium.Models.Dtos.Devices;
using Lockium.Models.Queries.Devices.Devices;
using Lockium.Mappings.Devices;
using Lockium.Data.LockiumDb.DatabaseContext;

namespace Lockium.Devices.Controllers.Devices
{
    [Route("/api/v1/devices")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdministrator,Administrator")]
    public partial class DevicesController : RestControllerBase2<Device, long, DeviceDto, DeviceQuery, DeviceMap>
    {
        public DevicesController(ILogger<RestServiceBase<Device, long>> logger,
            IDapperDbContext restDapperDb,
            LockiumDbContext restDb,
            DeviceMap deviceMap)
            : base(logger,
                restDapperDb,
                restDb,
                "Devices",
                deviceMap)
        {
        }

        /// <summary>
        /// Search of Device using given query
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">List of devices</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/devices/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<DeviceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<DeviceDto>> SearchAsync([FromBody] DeviceQuery query)
        {
            return await SearchUsingEfAsync(query, null, apply: LoadChannelsAsync);
        }

        /// <summary>
        /// Get the device by id
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Device data</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/devices/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<DeviceDto> FindAsync([FromRoute] long key)
        {
            return await FindUsingEfAsync(key, null, apply: LoadChannelsAsync);
        }

        private async Task LoadChannelsAsync(List<Device> devices)
        {
            if (devices.Count == 0)
                return;

            var deviceIds = devices.Select(d => d.Id).ToList();
            var channels = await restDb.Set<Channel>()
                .AsNoTracking()
                .Where(c => c.DeviceId != null && deviceIds.Contains(c.DeviceId.Value))
                .ToListAsync();

            var channelsByDeviceId = channels
                .GroupBy(c => c.DeviceId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var device in devices)
            {
                device.Channels = channelsByDeviceId.TryGetValue(device.Id, out var list)
                    ? list
                    : [];
            }
        }

    }
}
