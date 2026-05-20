using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Devices;

namespace Lockium.Models.Queries.Devices.Channels
{
    public partial class ChannelSort : SortBase<Channel>
    {
        public SortOperand? Id { get; set; }
        /// <summary>
        /// Номер ячейки
        /// </summary>
        public SortOperand? Number { get; set; }
        /// <summary>
        /// Статус: 1 - свободна, 2 - забронирована, 3 - занята
        /// </summary>
        public SortOperand? State { get; set; }
        /// <summary>
        /// Статус замка: 1 - закрыт, 2 - открыт
        /// </summary>
        public SortOperand? LockState { get; set; }
        /// <summary>
        /// Атрибуты: Width, Height, Length
        /// </summary>
        public SortOperand? Attributes { get; set; }
        public SortOperand? DeviceId { get; set; }
    }
}
