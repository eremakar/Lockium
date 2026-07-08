
namespace Lockium.Models.Dtos.Devices
{
    public partial class IRChannelDto
    {
        public long Id { get; set; }
        /// <summary>
        /// Номер ячейки
        /// </summary>
        public string? Number { get; set; }
        /// <summary>
        /// Статус
        /// </summary>
        public int State { get; set; }
        public long? BoardId { get; set; }

        public BoardDto? Board { get; set; }
    }
}
