using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Lockers;

namespace Lockium.Models.Queries.Lockers.Lockers
{
    /// <summary>
    /// Шкаф (постамат и т.п.)
    /// </summary>
    public partial class LockerSort : SortBase<Locker>
    {
        public SortOperand? Id { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        public SortOperand? Name { get; set; }
        /// <summary>
        /// Адрес
        /// </summary>
        public SortOperand? Address { get; set; }
        /// <summary>
        /// Тип: 1 - постамат
        /// </summary>
        public SortOperand? Type { get; set; }
    }
}
