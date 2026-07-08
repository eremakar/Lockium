using Lockium.Models.Dtos.Devices;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Lockium.Client.Api.Controllers.Devices
{
    public partial class DevicesController
    {
        [NonAction]
        public override Task<object> AddAsync(DeviceDto request) =>
            throw new NotSupportedException();

        [NonAction]
        public override Task<object> UpdateAsync(DeviceDto request) =>
            throw new NotSupportedException();

        [NonAction]
        public override Task<object> RemoveAsync(long key) =>
            throw new NotSupportedException();

        [NonAction]
        public override Task<IActionResult> PatchAsync(long id, JsonPatchDocument<DeviceDto> patch) =>
            throw new NotSupportedException();
    }
}
