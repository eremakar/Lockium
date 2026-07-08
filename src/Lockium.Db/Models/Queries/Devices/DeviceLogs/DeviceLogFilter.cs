using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Devices;

namespace Lockium.Models.Queries.Devices.DeviceLogs
{
    /// <summary>
    /// Лог команд и событий устройства
    /// </summary>
    public partial class DeviceLogFilter : FilterBase<DeviceLog>
    {
        public FilterOperand<long>? Id { get; set; }
        /// <summary>
        /// Время
        /// </summary>
        public FilterOperand<DateTime>? Time { get; set; }
        /// <summary>
        /// Тип записи: 1 - команда, 2 - событие
        /// </summary>
        public FilterOperand<int>? RecordType { get; set; }
        /// <summary>
        /// Имя команды или события
        /// </summary>
        public FilterOperand<string>? Name { get; set; }
        /// <summary>
        /// Данные команды или события
        /// </summary>
        public FilterOperand<object>? Payload { get; set; }
        /// <summary>
        /// Статус: 1 - успешно, 2 - ошибка
        /// </summary>
        public FilterOperand<int>? State { get; set; }
        /// <summary>
        /// Текст ошибки
        /// </summary>
        public FilterOperand<string>? ErrorMessage { get; set; }
        public FilterOperand<long?>? DeviceId { get; set; }
        public FilterOperand<long?>? BoardId { get; set; }
        public FilterOperand<long?>? ChannelId { get; set; }
    }
}
