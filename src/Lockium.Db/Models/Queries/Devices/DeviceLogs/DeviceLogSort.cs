using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Devices;

namespace Lockium.Models.Queries.Devices.DeviceLogs
{
    /// <summary>
    /// Лог команд и событий устройства
    /// </summary>
    public partial class DeviceLogSort : SortBase<DeviceLog>
    {
        public SortOperand? Id { get; set; }
        /// <summary>
        /// Время
        /// </summary>
        public SortOperand? Time { get; set; }
        /// <summary>
        /// Тип записи: 1 - команда, 2 - событие
        /// </summary>
        public SortOperand? RecordType { get; set; }
        /// <summary>
        /// Имя команды или события
        /// </summary>
        public SortOperand? Name { get; set; }
        /// <summary>
        /// Данные команды или события
        /// </summary>
        public SortOperand? Payload { get; set; }
        /// <summary>
        /// Статус: 1 - успешно, 2 - ошибка
        /// </summary>
        public SortOperand? State { get; set; }
        /// <summary>
        /// Текст ошибки
        /// </summary>
        public SortOperand? ErrorMessage { get; set; }
        public SortOperand? DeviceId { get; set; }
        public SortOperand? BoardId { get; set; }
        public SortOperand? ChannelId { get; set; }
    }
}
