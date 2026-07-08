using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Billings;

namespace Lockium.Models.Queries.Billings.Billings
{
    /// <summary>
    /// Данные по времени брони и заказа
    /// </summary>
    public partial class BillingSort : SortBase<Billing>
    {
        public SortOperand? Id { get; set; }
        /// <summary>
        /// Время начала
        /// </summary>
        public SortOperand? StartTime { get; set; }
        /// <summary>
        /// Время окончания
        /// </summary>
        public SortOperand? EndTime { get; set; }
        /// <summary>
        /// Сумма
        /// </summary>
        public SortOperand? Amount { get; set; }
        /// <summary>
        /// Потраченное время (в единицах TimeUnit)
        /// </summary>
        public SortOperand? Duration { get; set; }
        /// <summary>
        /// Транзакция
        /// </summary>
        public SortOperand? TransactionId { get; set; }
        /// <summary>
        /// Бронь
        /// </summary>
        public SortOperand? ReservationId { get; set; }
        /// <summary>
        /// Заказ
        /// </summary>
        public SortOperand? OrderId { get; set; }
    }
}
