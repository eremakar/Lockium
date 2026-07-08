using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Devices;

namespace Lockium.Models.Queries.Devices.IRChannels
{
    public partial class IRChannelFilter : FilterBase<IRChannel>
    {
        public FilterOperand<long>? Id { get; set; }
        /// <summary>
        /// Номер ячейки
        /// </summary>
        public FilterOperand<string>? Number { get; set; }
        /// <summary>
        /// Статус
        /// </summary>
        public FilterOperand<int>? State { get; set; }
        public FilterOperand<long?>? BoardId { get; set; }
    }
}
