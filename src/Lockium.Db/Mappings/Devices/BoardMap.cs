using Data.Mapping;
using Lockium.Data.LockiumDb.Entities.Devices;
using Lockium.Models.Dtos.Devices;

namespace Lockium.Mappings.Devices
{
    public partial class BoardMap : MapBase2<Board, BoardDto, MapOptions>
    {
        private readonly DbMapContext mapContext;

        public BoardMap(DbMapContext mapContext)
        {
            this.mapContext = mapContext;
        }

        public override BoardDto MapCore(Board source, MapOptions? options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new BoardDto();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.Name = source.Name;
                result.IsChannel = source.IsChannel;
                result.IsIR = source.IsIR;
                result.DeviceId = source.DeviceId;
                result.UpId = source.UpId;
            }
            if (options.MapObjects)
            {
                result.Device = mapContext.DeviceMap.Map(source.Device, options);
                result.Up = mapContext.BoardMap.Map(source.Up, options);
            }
            if (options.MapCollections)
            {
                result.Channels = mapContext.ChannelMap.Map(source.Channels, options);
                result.IRs = mapContext.IRChannelMap.Map(source.IRs, options);
            }

            return result;
        }

        public override Board ReverseMapCore(BoardDto source, MapOptions options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new Board();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.Name = source.Name;
                result.IsChannel = source.IsChannel;
                result.IsIR = source.IsIR;
                result.DeviceId = source.DeviceId;
                result.UpId = source.UpId;
            }
            if (options.MapObjects)
            {
                if (source.DeviceId == null)
                    result.Device = mapContext.DeviceMap.ReverseMap(source.Device, options);
                if (source.UpId == null)
                    result.Up = mapContext.BoardMap.ReverseMap(source.Up, options);
            }
            if (options.MapCollections)
            {
                result.Channels = mapContext.ChannelMap.ReverseMap(source.Channels, options);
                result.IRs = mapContext.IRChannelMap.ReverseMap(source.IRs, options);
            }

            return result;
        }

        public override void MapCore(Board source, Board destination, MapOptions options = null)
        {
            if (source == null || destination == null)
                return;

            options = options ?? new MapOptions();

            destination.Id = source.Id;
            if (options.MapProperties)
            {
                destination.Name = source.Name;
                destination.IsChannel = source.IsChannel;
                destination.IsIR = source.IsIR;
                destination.DeviceId = source.DeviceId;
                destination.UpId = source.UpId;
            }
            if (options.MapObjects)
            {
            }
            if (options.MapCollections)
            {
            }

        }
    }
}
