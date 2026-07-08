using Data.Repository;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lockium.Data.LockiumDb.Entities.Lockers
{
    /// <summary>
    /// Шкаф (постамат и т.п.)
    /// </summary>
    public partial class Locker : IEntityKey<long>
    {
        public long Id { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Адрес
        /// </summary>
        public string? Address { get; set; }
        /// <summary>
        /// Тип: 1 - постамат
        /// </summary>
        public int Type { get; set; }

        [InverseProperty("Locker")]
        public List<Cell>? Cells { get; set; }
    }
}
