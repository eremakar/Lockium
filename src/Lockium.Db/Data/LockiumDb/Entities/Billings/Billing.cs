using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Transactions;
using Lockium.Data.LockiumDb.Entities.Reservations;
using Lockium.Data.LockiumDb.Entities.Orders;

namespace Lockium.Data.LockiumDb.Entities.Billings
{
    /// <summary>
    /// Данные по времени брони и заказа
    /// </summary>
    public partial class Billing : IEntityKey<long>
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

        public Transaction? Transaction { get; set; }
        public Reservation? Reservation { get; set; }
        public Order? Order { get; set; }
    }
}
