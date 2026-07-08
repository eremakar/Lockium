using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Billings;

namespace Lockium.Models.Queries.Billings.Billings
{
    /// <summary>
    /// Данные по времени брони и заказа
    /// </summary>
    public partial class BillingFilter : FilterBase<Billing>
    {
        public FilterOperand<long>? Id { get; set; }
        /// <summary>
        /// Время начала
        /// </summary>
        public FilterOperand<DateTime>? StartTime { get; set; }
        /// <summary>
        /// Время окончания
        /// </summary>
        public FilterOperand<DateTime>? EndTime { get; set; }
        /// <summary>
        /// Сумма
        /// </summary>
        public FilterOperand<double>? Amount { get; set; }
        /// <summary>
        /// Потраченное время (в единицах TimeUnit)
        /// </summary>
        public FilterOperand<int>? Duration { get; set; }
        /// <summary>
        /// Транзакция
        /// </summary>
        public FilterOperand<long?>? TransactionId { get; set; }
        /// <summary>
        /// Бронь
        /// </summary>
        public FilterOperand<long?>? ReservationId { get; set; }
        /// <summary>
        /// Заказ
        /// </summary>
        public FilterOperand<long?>? OrderId { get; set; }
    }
}
