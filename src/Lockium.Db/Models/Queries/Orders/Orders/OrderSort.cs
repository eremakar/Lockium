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
        public SortOperand? CreatedTime { get; set; }
        /// <summary>
        /// PIN-код для получения посылки
        /// </summary>
        public SortOperand? PinCode { get; set; }
        /// <summary>
        /// Открытие для размещения уже использовано
        /// </summary>
        public SortOperand? DepositOpened { get; set; }
        /// <summary>
        /// Открытие для получения уже использовано
        /// </summary>
        public SortOperand? PickupOpened { get; set; }
        /// <summary>
        /// Номер отслеживания
        /// </summary>
        public SortOperand? TrackingNumber { get; set; }
        /// <summary>
        /// Срок хранения
        /// </summary>
        public SortOperand? ExpiresAt { get; set; }
        /// <summary>
        /// Данные о получателе
        /// </summary>
        public SortOperand? Recipient { get; set; }
        /// <summary>
        /// Клиент
        /// </summary>
        public SortOperand? ClientId { get; set; }
        /// <summary>
        /// Шкаф
        /// </summary>
        public SortOperand? LockerId { get; set; }
        /// <summary>
        /// Ячейка шкафа
        /// </summary>
        public SortOperand? CellId { get; set; }
        /// <summary>
        /// Канал платы замка
        /// </summary>
        public SortOperand? ChannelId { get; set; }
    }
}
