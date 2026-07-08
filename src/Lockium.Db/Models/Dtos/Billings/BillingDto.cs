using Lockium.Models.Dtos.Transactions;
using Lockium.Models.Dtos.Reservations;
using Lockium.Models.Dtos.Orders;

namespace Lockium.Models.Dtos.Billings
{
    /// <summary>
    /// Данные по времени брони и заказа
    /// </summary>
    public partial class BillingDto
    {
        public long Id { get; set; }
        /// <summary>
        /// Время начала
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// Время окончания
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// Сумма
        /// </summary>
        public double Amount { get; set; }
        /// <summary>
        /// Тариф за единицу времени
        /// </summary>
        public double Rate { get; set; }
        /// <summary>
        /// Единица времени: 1 - минута, 2 - час
        /// </summary>
        public int TimeUnit { get; set; }
        /// <summary>
        /// Потраченное время (в единицах TimeUnit)
        /// </summary>
        public int Duration { get; set; }
        /// <summary>
        /// Транзакция
        /// </summary>
        public long? TransactionId { get; set; }
        /// <summary>
        /// Бронь
        /// </summary>
        public long? ReservationId { get; set; }
        /// <summary>
        /// Заказ
        /// </summary>
        public long? OrderId { get; set; }

        /// <summary>
        /// Транзакция
        /// </summary>
        public TransactionDto? Transaction { get; set; }
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
