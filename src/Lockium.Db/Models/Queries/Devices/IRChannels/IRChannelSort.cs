using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Devices;

namespace Lockium.Models.Queries.Devices.IRChannels
{
    public partial class IRChannelSort : SortBase<IRChannel>
    {
        public SortOperand? Id { get; set; }
        /// <summary>
        /// Номер ячейки
        /// </summary>
        public SortOperand? Number { get; set; }
        /// <summary>
        /// Статус
        /// </summary>
        public SortOperand? State { get; set; }
        public SortOperand? BoardId { get; set; }
    }
}
