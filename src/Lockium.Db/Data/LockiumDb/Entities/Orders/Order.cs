using Data.Repository;
using Lockium.Data.LockiumDb.Entities;
using Lockium.Data.LockiumDb.Entities.Devices;

namespace Lockium.Data.LockiumDb.Entities.Orders
{
    public partial class Order : IEntityKey<long>
    {
        public long Id { get; set; }
        /// <summary>
        /// Статус: 1 - создан, 2 - занят, 3 - выполнен
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// Клиент
        /// </summary>
        public int? ClientId { get; set; }
        /// <summary>
        /// Ячейка
        /// </summary>
        public long? ChannelId { get; set; }

        public User? Client { get; set; }
        public Channel? Channel { get; set; }
    }
}
