using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Reservations;

namespace Lockium.Models.Queries.Reservations.Reservations
{
    public partial class ReservationFilter : FilterBase<Reservation>
    {
        public FilterOperand<long>? Id { get; set; }
        /// <summary>
        /// Статус: 1 - активна, 2 - снята
        /// </summary>
        public FilterOperand<int>? State { get; set; }
        public FilterOperand<DateTime>? CreatedTime { get; set; }
        /// <summary>
        /// Данные о получателе
        /// </summary>
        public FilterOperand<object>? Recipient { get; set; }
        /// <summary>
        /// Клиент
        /// </summary>
        public FilterOperand<int?>? ClientId { get; set; }
        /// <summary>
        /// Ячейка шкафа
        /// </summary>
        public FilterOperand<long?>? CellId { get; set; }
        /// <summary>
        /// Канал платы замка
        /// </summary>
        public FilterOperand<long?>? ChannelId { get; set; }
    }
}
