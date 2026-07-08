using Lockium.Models.Dtos;
using Lockium.Models.Dtos.Reservations;
using Lockium.Models.Dtos.Orders;

namespace Lockium.Models.Dtos.Transactions
{
    /// <summary>
    /// Транзакция, создаётся при брони или заказе
    /// </summary>
    public partial class TransactionDto
    {
        public long Id { get; set; }
        /// <summary>
        /// Статус: 1 - активна, 2 - завершена
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// Тип источника: 1 - бронь, 2 - заказ
        /// </summary>
        public int SourceType { get; set; }
        public DateTime CreatedTime { get; set; }
        /// <summary>
        /// Клиент
        /// </summary>
        public int? ClientId { get; set; }
        /// <summary>
        /// Бронь
        /// </summary>
        public long? ReservationId { get; set; }
        /// <summary>
        /// Заказ
        /// </summary>
        public long? OrderId { get; set; }

        /// <summary>
        /// Клиент
        /// </summary>
        public UserDto? Client { get; set; }
        /// <summary>
        /// Бронь
        /// </summary>
        public ReservationDto? Reservation { get; set; }
        /// <summary>
        /// Заказ
        /// </summary>
        public OrderDto? Order { get; set; }
    }
}
