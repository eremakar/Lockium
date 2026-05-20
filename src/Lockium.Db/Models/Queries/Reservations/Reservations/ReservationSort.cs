using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Reservations;

namespace Lockium.Models.Queries.Reservations.Reservations
{
    public partial class ReservationSort : SortBase<Reservation>
    {
        public SortOperand? Id { get; set; }
        /// <summary>
        /// Статус: 1 - активна, 2 - снята
        /// </summary>
        public SortOperand? State { get; set; }
        /// <summary>
        /// Клиент
        /// </summary>
        public SortOperand? ClientId { get; set; }
        /// <summary>
        /// Ячейка
        /// </summary>
        public SortOperand? ChannelId { get; set; }
    }
}
