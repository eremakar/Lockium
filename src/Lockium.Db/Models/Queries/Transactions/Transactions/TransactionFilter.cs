using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Transactions;

namespace Lockium.Models.Queries.Transactions.Transactions
{
    /// <summary>
    /// Транзакция, создаётся при брони или заказе
    /// </summary>
    public partial class TransactionFilter : FilterBase<Transaction>
    {
        public FilterOperand<long>? Id { get; set; }
        /// <summary>
        /// Статус: 1 - активна, 2 - завершена
        /// </summary>
        public FilterOperand<int>? State { get; set; }
        /// <summary>
        /// Тип источника: 1 - бронь, 2 - заказ
        /// </summary>
        public FilterOperand<int>? SourceType { get; set; }
        public FilterOperand<DateTime>? CreatedTime { get; set; }
        /// <summary>
        /// Клиент
        /// </summary>
        public FilterOperand<int?>? ClientId { get; set; }
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
