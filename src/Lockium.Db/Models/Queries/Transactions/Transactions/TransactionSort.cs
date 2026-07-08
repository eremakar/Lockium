using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Transactions;

namespace Lockium.Models.Queries.Transactions.Transactions
{
    /// <summary>
    /// Транзакция, создаётся при брони или заказе
    /// </summary>
    public partial class TransactionSort : SortBase<Transaction>
    {
        public SortOperand? Id { get; set; }
        /// <summary>
        /// Статус: 1 - активна, 2 - завершена
        /// </summary>
        public SortOperand? State { get; set; }
        /// <summary>
        /// Тип источника: 1 - бронь, 2 - заказ
        /// </summary>
        public SortOperand? SourceType { get; set; }
        public SortOperand? CreatedTime { get; set; }
        /// <summary>
        /// Клиент
        /// </summary>
        public SortOperand? ClientId { get; set; }
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
