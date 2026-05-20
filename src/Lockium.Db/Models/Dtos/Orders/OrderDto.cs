using Lockium.Models.Dtos;
using Lockium.Models.Dtos.Devices;

namespace Lockium.Models.Dtos.Orders
{
    public partial class OrderDto
    {
        public long Id { get; set; }
        /// <summary>
        /// Статус: 1 - создан, 2 - занят, 3 - выполнен
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// Клиент
        /// </summary>
        public int? ClientId { get; set; }
        /// <summary>
        /// Ячейка
        /// </summary>
        public long? ChannelId { get; set; }

        /// <summary>
        /// Клиент
        /// </summary>
        public UserDto? Client { get; set; }
        /// <summary>
        /// Ячейка
        /// </summary>
        public ChannelDto? Channel { get; set; }
    }
}
