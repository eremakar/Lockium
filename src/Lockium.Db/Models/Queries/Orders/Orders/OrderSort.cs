using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Orders;

namespace Lockium.Models.Queries.Orders.Orders
{
    public partial class OrderSort : SortBase<Order>
    {
        public SortOperand? Id { get; set; }
        /// <summary>
        /// Статус: 1 - создан, 2 - занят, 3 - выполнен
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
