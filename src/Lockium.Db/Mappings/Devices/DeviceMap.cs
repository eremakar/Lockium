using Data.Mapping;
using Lockium.Data.LockiumDb.Entities.Devices;
using Lockium.Models.Dtos.Devices;

namespace Lockium.Mappings.Devices
{
    public partial class DeviceMap : MapBase2<Device, DeviceDto, MapOptions>
    {
        private readonly DbMapContext mapContext;

        public DeviceMap(DbMapContext mapContext)
        {
            this.mapContext = mapContext;
        }

        public override DeviceDto MapCore(Device source, MapOptions? options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new DeviceDto();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.Name = source.Name;
                result.ConnectionState = source.ConnectionState;
            }
            if (options.MapObjects)
            {
            }
            if (options.MapCollections)
            {
                result.Channels = mapContext.ChannelMap.Map(source.Channels, options);
            }

            return result;
        }

        public override Device ReverseMapCore(DeviceDto source, MapOptions options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new Device();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.Name = source.Name;
                result.ConnectionState = source.ConnectionState;
            }
            if (options.MapObjects)
            {
            }
            if (options.MapCollections)
            {
                result.Channels = mapContext.ChannelMap.ReverseMap(source.Channels, options);
            }

            return result;
        }

        public override void MapCore(Device source, Device destination, MapOptions options = null)
        {
            if (source == null || destination == null)
                return;

            options = options ?? new MapOptions();

            destination.Id = source.Id;
            if (options.MapProperties)
            {
                destination.Name = source.Name;
                destination.ConnectionState = source.ConnectionState;
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
