using Data.Repository;

namespace Lockium.Data.LockiumDb.Entities.Devices
{
    public partial class IRChannel : IEntityKey<long>
    {
        public long Id { get; set; }
        /// <summary>
        /// Номер ячейки
        /// </summary>
        public string? Number { get; set; }
        /// <summary>
        /// Статус
        /// </summary>
        public int State { get; set; }
        public long? BoardId { get; set; }

        public Board? Board { get; set; }
    }
}
