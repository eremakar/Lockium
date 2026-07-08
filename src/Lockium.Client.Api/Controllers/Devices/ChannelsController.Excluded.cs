using Lockium.Models.Dtos.Devices;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Lockium.Client.Api.Controllers.Devices
{
    public partial class ChannelsController
    {
        [NonAction]
        public override Task<object> AddAsync(ChannelDto request) =>
            throw new NotSupportedException();

        [NonAction]
        public override Task<object> UpdateAsync(ChannelDto request) =>
            throw new NotSupportedException();

        [NonAction]
        public override Task<object> RemoveAsync(long key) =>
            throw new NotSupportedException();

        [NonAction]
        public override Task<IActionResult> PatchAsync(long id, JsonPatchDocument<ChannelDto> patch) =>
            throw new NotSupportedException();
    }
}
