using System.ComponentModel.DataAnnotations.Schema;
using Data.Repository;
using Lockium.Data.LockiumDb.Entities;
using Lockium.Data.LockiumDb.Entities.Lockers;
using Lockium.Data.LockiumDb.Entities.Devices;

namespace Lockium.Data.LockiumDb.Entities.Orders
{
    public partial class Order : IEntityKey<long>
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
        [Column(TypeName = "jsonb")]
        public string? Recipient { get; set; }
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

        public User? Client { get; set; }
        public Locker? Locker { get; set; }
        public Cell? Cell { get; set; }
        public Channel? Channel { get; set; }
    }
}
