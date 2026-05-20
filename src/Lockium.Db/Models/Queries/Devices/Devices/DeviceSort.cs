using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Devices;

namespace Lockium.Models.Queries.Devices.Devices
{
    public partial class DeviceSort : SortBase<Device>
    {
        public SortOperand? Id { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        public SortOperand? Name { get; set; }
        /// <summary>
        /// Статус подключения: 1 - выключен, 2 - включен, 3 - ошибка
        /// </summary>
        public SortOperand? ConnectionState { get; set; }
    }
}
