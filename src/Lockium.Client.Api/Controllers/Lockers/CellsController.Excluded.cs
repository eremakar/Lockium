using Lockium.Models.Dtos.Lockers;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Lockium.Client.Api.Controllers.Lockers
{
    public partial class CellsController
    {
        [NonAction]
        public override Task<object> AddAsync(CellDto request) =>
            throw new NotSupportedException();

        [NonAction]
        public override Task<object> UpdateAsync(CellDto request) =>
            throw new NotSupportedException();

        [NonAction]
        public override Task<object> RemoveAsync(long key) =>
            throw new NotSupportedException();

        [NonAction]
        public override Task<IActionResult> PatchAsync(long id, JsonPatchDocument<CellDto> patch) =>
            throw new NotSupportedException();
    }
}
