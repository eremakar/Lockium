using Data.Repository;
using Lockium.Data.LockiumDb.Entities;
using Lockium.Data.LockiumDb.Entities.Reservations;
using Lockium.Data.LockiumDb.Entities.Orders;

namespace Lockium.Data.LockiumDb.Entities.Transactions
{
    /// <summary>
    /// Транзакция, создаётся при брони или заказе
    /// </summary>
    public partial class Transaction : IEntityKey<long>
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

        public User? Client { get; set; }
        public Reservation? Reservation { get; set; }
        public Order? Order { get; set; }
    }
}
