using Lockium.Models.Dtos;
using Lockium.Models.Dtos.Lockers;
using Lockium.Models.Dtos.Devices;

namespace Lockium.Models.Dtos.Reservations
{
    public partial class ReservationDto
    {
        public long Id { get; set; }
        /// <summary>
        /// Статус: 1 - активна, 2 - снята
        /// </summary>
        public int State { get; set; }
        public DateTime CreatedTime { get; set; }
        /// <summary>
        /// Данные о получателе
        /// </summary>
        public object? Recipient { get; set; }
        /// <summary>
        /// Клиент
        /// </summary>
        public int? ClientId { get; set; }
        /// <summary>
        /// Ячейка шкафа
        /// </summary>
        public long? CellId { get; set; }
        /// <summary>
        /// Канал платы замка
        /// </summary>
        public long? ChannelId { get; set; }

        /// <summary>
        /// Клиент
        /// </summary>
        public UserDto? Client { get; set; }
        /// <summary>
        /// Ячейка шкафа
        /// </summary>
        public CellDto? Cell { get; set; }
        /// <summary>
        /// Канал платы замка
        /// </summary>
        public ChannelDto? Channel { get; set; }
    }
}
