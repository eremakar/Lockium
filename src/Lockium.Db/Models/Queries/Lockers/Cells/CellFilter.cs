using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Lockers;

namespace Lockium.Models.Queries.Lockers.Cells
{
    /// <summary>
    /// Ячейка шкафа
    /// </summary>
    public partial class CellFilter : FilterBase<Cell>
    {
        public FilterOperand<long>? Id { get; set; }
        /// <summary>
        /// Номер ячейки
        /// </summary>
        public FilterOperand<string>? Number { get; set; }
        /// <summary>
        /// Статус: 1 - свободна, 2 - забронирована, 3 - занята
        /// </summary>
        public FilterOperand<int>? State { get; set; }
        /// <summary>
        /// Атрибуты: Width, Height, Length
        /// </summary>
        public FilterOperand<object>? Attributes { get; set; }
        /// <summary>
        /// Шкаф
        /// </summary>
        public FilterOperand<long?>? LockerId { get; set; }
        /// <summary>
        /// Канал платы замка
        /// </summary>
        public FilterOperand<long?>? ChannelId { get; set; }
    }
}
