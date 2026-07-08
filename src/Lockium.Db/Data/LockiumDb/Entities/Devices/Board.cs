using Data.Repository;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lockium.Data.LockiumDb.Entities.Devices
{
    public partial class Board : IEntityKey<long>
    {
        public long Id { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        public string? Name { get; set; }
        public bool IsChannel { get; set; }
        public bool IsIR { get; set; }
        public long? DeviceId { get; set; }
        public long? UpId { get; set; }

        public Device? Device { get; set; }
        public Board? Up { get; set; }

        [InverseProperty("Board")]
        public List<Channel>? Channels { get; set; }
        [InverseProperty("Board")]
        public List<IRChannel>? IRs { get; set; }
    }
}
