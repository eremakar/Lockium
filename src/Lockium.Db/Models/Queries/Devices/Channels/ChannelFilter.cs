using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Devices;

namespace Lockium.Models.Queries.Devices.Channels
{
    public partial class ChannelFilter : FilterBase<Channel>
    {
        public FilterOperand<long>? Id { get; set; }
        /// <summary>
        /// Номер ячейки
        /// </summary>
        public FilterOperand<string>? Number { get; set; }
        /// <summary>
        /// Статус: 1 - свободна, 2 - забронирована, 3 - занята
        /// </summary>
        public FilterOperand<int>? State { get; set; }
        /// <summary>
        /// Статус замка: 1 - закрыт, 2 - открыт
        /// </summary>
        public FilterOperand<int>? LockState { get; set; }
        /// <summary>
        /// Атрибуты: Width, Height, Length
        /// </summary>
        public FilterOperand<object>? Attributes { get; set; }
        public FilterOperand<long?>? DeviceId { get; set; }
        public FilterOperand<long?>? BoardId { get; set; }
    }
}
