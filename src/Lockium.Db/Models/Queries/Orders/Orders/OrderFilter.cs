using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Orders;

namespace Lockium.Models.Queries.Orders.Orders
{
    public partial class OrderFilter : FilterBase<Order>
    {
        public FilterOperand<long>? Id { get; set; }
        /// <summary>
        /// Статус: 1 - создан, 2 - занят, 3 - выполнен
        /// </summary>
        public FilterOperand<int>? State { get; set; }
        /// <summary>
        /// Клиент
        /// </summary>
        public FilterOperand<int?>? ClientId { get; set; }
        /// <summary>
        /// Ячейка
        /// </summary>
        public FilterOperand<long?>? ChannelId { get; set; }
    }
}
