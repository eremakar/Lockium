
namespace Lockium.Models.Dtos.Devices
{
    /// <summary>
    /// Лог команд и событий устройства
    /// </summary>
    public partial class DeviceLogDto
    {
        public long Id { get; set; }
        /// <summary>
        /// Время
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// Тип записи: 1 - команда, 2 - событие
        /// </summary>
        public int RecordType { get; set; }
        /// <summary>
        /// Имя команды или события
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Данные команды или события
        /// </summary>
        public object? Payload { get; set; }
        /// <summary>
        /// Статус: 1 - успешно, 2 - ошибка
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// Текст ошибки
        /// </summary>
        public string? ErrorMessage { get; set; }
        public long? DeviceId { get; set; }
        public long? BoardId { get; set; }
        public long? ChannelId { get; set; }

        public DeviceDto? Device { get; set; }
        public BoardDto? Board { get; set; }
        public ChannelDto? Channel { get; set; }
    }
}
