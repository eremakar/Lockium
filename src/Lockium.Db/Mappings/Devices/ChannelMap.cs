using Data.Mapping;
using Lockium.Data.LockiumDb.Entities.Devices;
using Lockium.Models.Dtos.Devices;
using Newtonsoft.Json;
using Data.Repository.Helpers;

namespace Lockium.Mappings.Devices
{
    public partial class ChannelMap : MapBase2<Channel, ChannelDto, MapOptions>
    {
        private readonly DbMapContext mapContext;

        public ChannelMap(DbMapContext mapContext)
        {
            this.mapContext = mapContext;
        }

        public override ChannelDto MapCore(Channel source, MapOptions? options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new ChannelDto();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.Number = source.Number;
                result.State = source.State;
                result.LockState = source.LockState;
                result.Attributes = source.Attributes;
                result.DeviceId = source.DeviceId;
                result.BoardId = source.BoardId;
            }
            if (options.MapObjects)
            {
                result.Device = mapContext.DeviceMap.Map(source.Device, options);
                result.Board = mapContext.BoardMap.Map(source.Board, options);
            }
            if (options.MapCollections)
            {
            }

            return result;
        }

        public override Channel ReverseMapCore(ChannelDto source, MapOptions options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new Channel();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.Number = source.Number;
                result.State = source.State;
                result.LockState = source.LockState;
                if (source.Attributes != null)
                    result.Attributes = JsonConvert.SerializeObject(source.Attributes);
                result.DeviceId = source.DeviceId;
                result.BoardId = source.BoardId;
            }
            if (options.MapObjects)
            {
                if (source.DeviceId == null)
                    result.Device = mapContext.DeviceMap.ReverseMap(source.Device, options);
                if (source.BoardId == null)
                    result.Board = mapContext.BoardMap.ReverseMap(source.Board, options);
            }
            if (options.MapCollections)
            {
            }

            return result;
        }

        public override void MapCore(Channel source, Channel destination, MapOptions options = null)
        {
            if (source == null || destination == null)
                return;

            options = options ?? new MapOptions();

            destination.Id = source.Id;
            if (options.MapProperties)
            {
                destination.Number = source.Number;
                destination.State = source.State;
                destination.LockState = source.LockState;
                destination.Attributes = JsonHelper.NormalizeSafe(source.Attributes);
                destination.DeviceId = source.DeviceId;
                destination.BoardId = source.BoardId;
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
