using Data.Repository;
using Data.Repository.Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;
using Lockium.Data.LockiumDb.Entities.Reservations;
using Lockium.Models.Dtos.Reservations;
using Lockium.Models.Queries.Reservations.Reservations;
using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Mappings.Reservations;

namespace Lockium.Reservations.Controllers.Reservations
{
    [Route("/api/v1/reservations")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdministrator,Administrator")]
    public partial class ReservationsController : RestControllerBase2<Reservation, long, ReservationDto, ReservationQuery, ReservationMap>
    {
        public ReservationsController(ILogger<RestServiceBase<Reservation, long>> logger,
            IDapperDbContext restDapperDb,
            LockiumDbContext restDb,
            ReservationMap reservationMap)
            : base(logger,
                restDapperDb,
                restDb,
                "Reservations",
                reservationMap)
        {
        }

        /// <summary>
        /// Search of Reservation using given query
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">List of reservations</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/reservations/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<ReservationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<ReservationDto>> SearchAsync([FromBody] ReservationQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Client).
                Include(_ => _.Channel));
        }

        /// <summary>
        /// Get the reservation by id
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Reservation data</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/reservations/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<ReservationDto> FindAsync([FromRoute] long key)
        {
            return await FindUsingEfAsync(key, _ => _.
                Include(_ => _.Client).
                Include(_ => _.Channel));
        }

    }
}
