
namespace Lockium.Models.Dtos.Lockers
{
    /// <summary>
    /// Шкаф (постамат и т.п.)
    /// </summary>
    public partial class LockerDto
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

        /// <summary>
        /// Ячейки
        /// </summary>
        public List<CellDto>? Cells { get; set; }
    }
}
