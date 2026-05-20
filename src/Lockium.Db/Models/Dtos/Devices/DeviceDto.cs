
namespace Lockium.Models.Dtos.Devices
{
    public partial class DeviceDto
    {
        public long Id { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Статус подключения: 1 - выключен, 2 - включен, 3 - ошибка
        /// </summary>
        public int ConnectionState { get; set; }

        /// <summary>
        /// Ячейки
        /// </summary>
        public List<ChannelDto>? Channels { get; set; }
    }
}
