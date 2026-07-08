using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using Lockium.Data.LockiumDb.Entities.Devices;
using Lockium.Data.LockiumDb.Entities.Lockers;
using Lockium.Models.Dtos.Lockers;

namespace Lockium.Lockers.Controllers.Lockers
{
    public partial class LockersController
    {
        /// <summary>
        /// Create locker and cells from device channels
        /// </summary>
        /// <remarks>
        /// Creates a locker from the device and a cell per channel, binding each cell to its channel.
        /// </remarks>
        /// <response code="200">Created locker with cells</response>
        /// <response code="400">Device has no channels or cells already exist for its channels</response>
        /// <response code="404">Device not found</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/lockers/device/{deviceId}")]
        [HttpPost]
        [ProducesResponseType(typeof(LockerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<LockerDto>> AddFromDeviceAsync([FromRoute] long deviceId)
        {
            var device = await restDb.Set<Device>()
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == deviceId);

            if (device == null)
                return NotFound();

            var channels = await restDb.Set<Channel>()
                .AsNoTracking()
                .Where(c => c.DeviceId == deviceId)
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (channels.Count == 0)
                return BadRequest("Device has no channels");

            var channelIds = channels.Select(c => c.Id).ToList();
            var linkedChannelExists = await restDb.Set<Cell>()
                .AnyAsync(c => c.ChannelId != null && channelIds.Contains(c.ChannelId.Value));

            if (linkedChannelExists)
                return BadRequest("Locker cells already exist for device channels");

            var locker = await InvokeDbAsync(async () =>
            {
                var entity = new Locker
                {
                    Name = device.Name,
                    Type = 1,
                    Cells = channels.Select(channel => new Cell
                    {
                        Number = channel.Number,
                        State = channel.State,
                        Attributes = channel.Attributes,
                        ChannelId = channel.Id,
                    }).ToList(),
                };

                await restSet.AddAsync(entity);
                await restDb.SaveChangesAsync();

                return entity;
            });

            return map.Map(locker)!;
        }

        /// <summary>
        /// Add new locker
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Unique registered id</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/lockers")]
        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<object> AddAsync([FromBody] LockerDto request)
        {
            return await base.AddAsync(request);
        }

        /// <summary>
        /// Update locker
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/lockers")]
        [HttpPut]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<object> UpdateAsync([FromBody] LockerDto request)
        {
            return await base.UpdateAsync(request);
        }

        /// <summary>
        /// Patch locker
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/lockers/patch")]
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<IActionResult> PatchAsync(long id, [FromBody] JsonPatchDocument<LockerDto> patch)
        {
            return await base.PatchAsync(id, patch);
        }

        /// <summary>
        /// Remove locker
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/lockers/{key}")]
        [HttpDelete]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<object> RemoveAsync([FromRoute] long key)
        {
            return await base.RemoveAsync(key);
        }

    }
}
