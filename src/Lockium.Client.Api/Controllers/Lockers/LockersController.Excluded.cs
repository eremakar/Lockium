using Lockium.Models.Dtos.Lockers;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Lockium.Client.Api.Controllers.Lockers
{
    public partial class LockersController
    {
        [NonAction]
        public override Task<object> AddAsync(LockerDto request) =>
            throw new NotSupportedException();

        [NonAction]
        public override Task<object> UpdateAsync(LockerDto request) =>
            throw new NotSupportedException();

        [NonAction]
        public override Task<object> RemoveAsync(long key) =>
            throw new NotSupportedException();

        [NonAction]
        public override Task<IActionResult> PatchAsync(long id, JsonPatchDocument<LockerDto> patch) =>
            throw new NotSupportedException();
    }
}
