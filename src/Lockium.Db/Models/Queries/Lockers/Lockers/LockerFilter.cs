using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Lockers;

namespace Lockium.Models.Queries.Lockers.Lockers
{
    /// <summary>
    /// Шкаф (постамат и т.п.)
    /// </summary>
    public partial class LockerFilter : FilterBase<Locker>
    {
        public FilterOperand<long>? Id { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        public FilterOperand<string>? Name { get; set; }
        /// <summary>
        /// Адрес
        /// </summary>
        public FilterOperand<string>? Address { get; set; }
        /// <summary>
        /// Тип: 1 - постамат
        /// </summary>
        public FilterOperand<int>? Type { get; set; }
    }
}
