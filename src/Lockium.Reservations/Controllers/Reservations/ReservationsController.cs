using Data.Repository;
using Data.Repository.Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;
using Lockium.Data.LockiumDb.Entities;
using Lockium.Data.LockiumDb.Entities.Devices;
using Lockium.Data.LockiumDb.Entities.Reservations;
using Lockium.Models.Dtos.Reservations;
using Lockium.Models.Queries.Reservations.Reservations;
using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Mappings.Reservations;
using Lockium.Workflows.Reservations;

namespace Lockium.Reservations.Controllers.Reservations
{
    [Route("/api/v1/reservations")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdministrator,Administrator")]
    public partial class ReservationsController : RestControllerBase2<Reservation, long, ReservationDto, ReservationQuery, ReservationMap>
    {
        private readonly IMicroserviceStateWorkflow stateWorkflow;

        public ReservationsController(ILogger<RestServiceBase<Reservation, long>> logger,
            IDapperDbContext restDapperDb,
            LockiumDbContext restDb,
            ReservationMap reservationMap,
            IMicroserviceStateWorkflow stateWorkflow)
            : base(logger,
                restDapperDb,
                restDb,
                "Reservations",
                reservationMap)
        {
            this.stateWorkflow = stateWorkflow;
        }

        private static IQueryable<Reservation> ReservationQuery(IQueryable<Reservation> query) =>
            query.Select(r => new Reservation
            {
                Id = r.Id,
                State = r.State,
                ClientId = r.ClientId,
                ChannelId = r.ChannelId,
                Client = r.Client == null
                    ? null
                    : new User
                    {
                        Id = r.Client.Id,
                        UserName = r.Client.UserName,
                    },
                Channel = r.Channel == null
                    ? null
                    : new Channel
                    {
                        Id = r.Channel.Id,
                        Number = r.Channel.Number,
                        State = r.Channel.State,
                        LockState = r.Channel.LockState,
                        Attributes = r.Channel.Attributes,
                        DeviceId = r.Channel.DeviceId,
                        Device = r.Channel.Device == null
                            ? null
                            : new Device
                            {
                                Id = r.Channel.Device.Id,
                                Name = r.Channel.Device.Name,
                                ConnectionState = r.Channel.Device.ConnectionState,
                            },
                    },
            });

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
            return await SearchUsingEfAsync(query, ReservationQuery);
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
            return await FindUsingEfAsync(key, ReservationQuery);
        }

    }
}
