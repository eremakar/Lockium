using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Reservations;

namespace Lockium.Models.Queries.Reservations.Reservations
{
    public partial class ReservationQuery : QueryBase<Reservation, ReservationFilter, ReservationSort>
    {
    }
}
