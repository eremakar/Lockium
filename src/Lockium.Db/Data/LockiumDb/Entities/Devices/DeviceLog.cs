using System.Text.Json;
using Data.Repository;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lockium.Data.LockiumDb.Entities.Devices
{
    /// <summary>
    /// Лог команд и событий устройства
    /// </summary>
    public partial class DeviceLog : IEntityKey<long>
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
        [Column(TypeName = "jsonb")]
        public string? Payload { get; set; }
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

        public Device? Device { get; set; }
        public Board? Board { get; set; }
        public Channel? Channel { get; set; }
    }
}
