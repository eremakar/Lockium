using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Devices;

namespace Lockium.Models.Queries.Devices.Devices
{
    public partial class DeviceFilter : FilterBase<Device>
    {
        public FilterOperand<long>? Id { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        public FilterOperand<string>? Name { get; set; }
        /// <summary>
        /// Статус подключения: 1 - выключен, 2 - включен, 3 - ошибка
        /// </summary>
        public FilterOperand<int>? ConnectionState { get; set; }
    }
}
