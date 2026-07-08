using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Lockers;

namespace Lockium.Models.Queries.Lockers.Cells
{
    /// <summary>
    /// Ячейка шкафа
    /// </summary>
    public partial class CellSort : SortBase<Cell>
    {
        public SortOperand? Id { get; set; }
        /// <summary>
        /// Номер ячейки
        /// </summary>
        public SortOperand? Number { get; set; }
        /// <summary>
        /// Статус: 1 - свободна, 2 - забронирована, 3 - занята
        /// </summary>
        public SortOperand? State { get; set; }
        /// <summary>
        /// Атрибуты: Width, Height, Length
        /// </summary>
        public SortOperand? Attributes { get; set; }
        /// <summary>
        /// Шкаф
        /// </summary>
        public SortOperand? LockerId { get; set; }
        /// <summary>
        /// Канал платы замка
        /// </summary>
        public SortOperand? ChannelId { get; set; }
    }
}
