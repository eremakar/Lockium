
namespace Lockium.Models.Dtos.Devices
{
    public partial class BoardDto
    {
        public long Id { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        public string? Name { get; set; }
        public bool IsChannel { get; set; }
        public bool IsIR { get; set; }
        public long? DeviceId { get; set; }
        public long? UpId { get; set; }

        public DeviceDto? Device { get; set; }
        public BoardDto? Up { get; set; }

        /// <summary>
        /// Ячейки
        /// </summary>
        public List<ChannelDto>? Channels { get; set; }
        /// <summary>
        /// IR
        /// </summary>
        public List<IRChannelDto>? IRs { get; set; }
    }
}
