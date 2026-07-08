using System.Text.Json;
using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Devices;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lockium.Data.LockiumDb.Entities.Lockers
{
    /// <summary>
    /// Ячейка шкафа
    /// </summary>
    public partial class Cell : IEntityKey<long>
    {
        public long Id { get; set; }
        /// <summary>
        /// Номер ячейки
        /// </summary>
        public string? Number { get; set; }
        /// <summary>
        /// Статус: 1 - свободна, 2 - забронирована, 3 - занята
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// Атрибуты: Width, Height, Length
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? Attributes { get; set; }
        /// <summary>
        /// Шкаф
        /// </summary>
        public long? LockerId { get; set; }
        /// <summary>
        /// Канал платы замка
        /// </summary>
        public long? ChannelId { get; set; }

        public Locker? Locker { get; set; }
        public Channel? Channel { get; set; }
    }
}
