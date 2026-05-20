
namespace Lockium.Models.Dtos.Devices
{
    public partial class ChannelDto
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
        /// Статус замка: 1 - закрыт, 2 - открыт
        /// </summary>
        public int LockState { get; set; }
        /// <summary>
        /// Атрибуты: Width, Height, Length
        /// </summary>
        public object? Attributes { get; set; }
        public long? DeviceId { get; set; }

        public DeviceDto? Device { get; set; }
    }
}
