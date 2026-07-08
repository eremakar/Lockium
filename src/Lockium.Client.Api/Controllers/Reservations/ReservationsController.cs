using Data.Repository;
using Data.Repository.Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Lockium.Data.LockiumDb.Entities.Reservations;
using Lockium.Models.Dtos.Reservations;
using Lockium.Models.Queries.Reservations.Reservations;
using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Mappings.Reservations;
using Lockium.Workflows.Reservations;

namespace Lockium.Client.Api.Controllers.Reservations
{
    /// <summary>
    /// Бронирование ячеек постамата.
    /// </summary>
    /// <remarks>
    /// Единственная публичная операция клиентского API — создание брони (<c>POST /api/v1/reservations</c>).
    /// Поиск, чтение, изменение и удаление через этот контроллер недоступны (методы скрыты атрибутом <c>NonAction</c>).
    /// </remarks>
    [Route("/api/v1/reservations")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Client,SuperAdministrator,Administrator")]
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
    }
}
