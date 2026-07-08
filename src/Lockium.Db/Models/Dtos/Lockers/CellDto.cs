using Lockium.Models.Dtos.Devices;

namespace Lockium.Models.Dtos.Lockers
{
    /// <summary>
    /// Ячейка шкафа
    /// </summary>
    public partial class CellDto
    {
        public long Id { get; set; }
        /// <summary>
        /// Номер ячейки
        /// </summary>
        public string? Number { get; set; }
        /// <summary>
        /// Статус: 1 - свободна, 2 - забронирована, 3 - занята
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// Атрибуты: Width, Height, Length
        /// </summary>
        public object? Attributes { get; set; }
        /// <summary>
        /// Шкаф
        /// </summary>
        public long? LockerId { get; set; }
        /// <summary>
        /// Канал платы замка
        /// </summary>
        public long? ChannelId { get; set; }

        /// <summary>
        /// Шкаф
        /// </summary>
        public LockerDto? Locker { get; set; }
        /// <summary>
        /// Канал платы замка
        /// </summary>
        public ChannelDto? Channel { get; set; }
    }
}
