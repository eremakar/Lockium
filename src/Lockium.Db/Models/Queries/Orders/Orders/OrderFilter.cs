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
        public FilterOperand<DateTime>? CreatedTime { get; set; }
        /// <summary>
        /// PIN-код для получения посылки
        /// </summary>
        public FilterOperand<string>? PinCode { get; set; }
        /// <summary>
        /// Открытие для размещения уже использовано
        /// </summary>
        public FilterOperand<bool>? DepositOpened { get; set; }
        /// <summary>
        /// Открытие для получения уже использовано
        /// </summary>
        public FilterOperand<bool>? PickupOpened { get; set; }
        /// <summary>
        /// Номер отслеживания
        /// </summary>
        public FilterOperand<string>? TrackingNumber { get; set; }
        /// <summary>
        /// Срок хранения
        /// </summary>
        public FilterOperand<DateTime>? ExpiresAt { get; set; }
        /// <summary>
        /// Клиент
        /// </summary>
        public FilterOperand<int?>? ClientId { get; set; }
        /// <summary>
        /// Шкаф
        /// </summary>
        public FilterOperand<long?>? LockerId { get; set; }
        /// <summary>
        /// Ячейка
        /// </summary>
        public FilterOperand<long?>? ChannelId { get; set; }
    }
}
