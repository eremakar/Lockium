using System.Text.Json;
using Data.Repository;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lockium.Data.LockiumDb.Entities.Devices
{
    public partial class Channel : IEntityKey<long>
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
        /// Статус замка: 1 - закрыт, 2 - открыт
        /// </summary>
        public int LockState { get; set; }
        /// <summary>
        /// Атрибуты: Width, Height, Length
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? Attributes { get; set; }
        public long? DeviceId { get; set; }
        public long? BoardId { get; set; }

        public Device? Device { get; set; }
        public Board? Board { get; set; }
    }
}
