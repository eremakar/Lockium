using Lockium.Models.Dtos;
using Lockium.Models.Dtos.Lockers;
using Lockium.Models.Dtos.Devices;

namespace Lockium.Models.Dtos.Orders
{
    public partial class OrderDto
    {
        public long Id { get; set; }
        /// <summary>
        /// Статус: 1 - создан, 2 - занят, 3 - выполнен
        /// </summary>
        public int State { get; set; }
        public DateTime CreatedTime { get; set; }
        /// <summary>
        /// PIN-код для получения посылки
        /// </summary>
        public string? PinCode { get; set; }
        /// <summary>
        /// Открытие для размещения уже использовано
        /// </summary>
        public bool DepositOpened { get; set; }
        /// <summary>
        /// Открытие для получения уже использовано
        /// </summary>
        public bool PickupOpened { get; set; }
        /// <summary>
        /// Номер отслеживания
        /// </summary>
        public string? TrackingNumber { get; set; }
        /// <summary>
        /// Срок хранения
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        /// <summary>
        /// Данные о получателе
        /// </summary>
        public object? Recipient { get; set; }
        /// <summary>
        /// Клиент
        /// </summary>
        public int? ClientId { get; set; }
        /// <summary>
        /// Шкаф
        /// </summary>
        public long? LockerId { get; set; }
        /// <summary>
        /// Ячейка шкафа
        /// </summary>
        public long? CellId { get; set; }
        /// <summary>
        /// Канал платы замка
        /// </summary>
        public long? ChannelId { get; set; }

        /// <summary>
        /// Клиент
        /// </summary>
        public UserDto? Client { get; set; }
        /// <summary>
        /// Шкаф
        /// </summary>
        public LockerDto? Locker { get; set; }
        /// <summary>
        /// Ячейка шкафа
        /// </summary>
        public CellDto? Cell { get; set; }
        /// <summary>
        /// Канал платы замка
        /// </summary>
        public ChannelDto? Channel { get; set; }
    }
}
