using Data.Repository;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lockium.Data.LockiumDb.Entities.Devices
{
    public partial class Device : IEntityKey<long>
    {
        public long Id { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Статус подключения: 1 - выключен, 2 - включен, 3 - ошибка
        /// </summary>
        public int ConnectionState { get; set; }

        [InverseProperty("Device")]
        public List<Channel>? Channels { get; set; }
        [InverseProperty("Device")]
        public List<DeviceLog>? Logs { get; set; }
    }
}
