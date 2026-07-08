using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Devices;

namespace Lockium.Models.Queries.Devices.Boards
{
    public partial class BoardSort : SortBase<Board>
    {
        public SortOperand? Id { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        public SortOperand? Name { get; set; }
        public SortOperand? DeviceId { get; set; }
        public SortOperand? UpId { get; set; }
    }
}
