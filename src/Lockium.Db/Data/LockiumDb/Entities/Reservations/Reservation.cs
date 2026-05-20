using Data.Repository;
using Lockium.Data.LockiumDb.Entities;
using Lockium.Data.LockiumDb.Entities.Devices;

namespace Lockium.Data.LockiumDb.Entities.Reservations
{
    public partial class Reservation : IEntityKey<long>
    {
        public long Id { get; set; }
        /// <summary>
        /// Статус: 1 - активна, 2 - снята
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
