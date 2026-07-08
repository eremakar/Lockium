using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Devices;

namespace Lockium.Models.Queries.Devices.Boards
{
    public partial class BoardFilter : FilterBase<Board>
    {
        public FilterOperand<long>? Id { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        public FilterOperand<string>? Name { get; set; }
        public FilterOperand<long?>? DeviceId { get; set; }
        public FilterOperand<long?>? UpId { get; set; }
    }
}
