using Data.Repository;
using Lockium.Models.Dtos.Reservations;
using Lockium.Models.Queries.Reservations.Reservations;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Lockium.Client.Api.Controllers.Reservations
{
    public partial class ReservationsController
    {
        [NonAction]
        public override Task<PagedList<ReservationDto>> SearchAsync(ReservationQuery query) =>
            throw new NotSupportedException();

        [NonAction]
        public override Task<ReservationDto> FindAsync(long key) =>
            throw new NotSupportedException();

        [NonAction]
        public override Task<object> AddAsync(ReservationDto request) =>
            throw new NotSupportedException();

        [NonAction]
        public override Task<object> UpdateAsync(ReservationDto request) =>
            throw new NotSupportedException();

        [NonAction]
        public override Task<object> RemoveAsync(long key) =>
            throw new NotSupportedException();

        [NonAction]
        public override Task<IActionResult> PatchAsync(long id, JsonPatchDocument<ReservationDto> patch) =>
            throw new NotSupportedException();
    }
}
