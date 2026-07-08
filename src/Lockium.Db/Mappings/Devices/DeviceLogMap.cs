using Data.Mapping;
using Data.Repository.Helpers;
using Lockium.Data.LockiumDb.Entities.Devices;
using Lockium.Models.Dtos.Devices;
using Newtonsoft.Json;
using Data.Repository.Helpers;

namespace Lockium.Mappings.Devices
{
    /// <summary>
    /// Лог команд и событий устройства
    /// </summary>
    public partial class DeviceLogMap : MapBase2<DeviceLog, DeviceLogDto, MapOptions>
    {
        private readonly DbMapContext mapContext;

        public DeviceLogMap(DbMapContext mapContext)
        {
            this.mapContext = mapContext;
        }

        public override DeviceLogDto MapCore(DeviceLog source, MapOptions? options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new DeviceLogDto();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.Time = source.Time;
                result.RecordType = source.RecordType;
                result.Name = source.Name;
                result.Payload = source.Payload;
                result.State = source.State;
                result.ErrorMessage = source.ErrorMessage;
                result.DeviceId = source.DeviceId;
                result.BoardId = source.BoardId;
                result.ChannelId = source.ChannelId;
            }
            if (options.MapObjects)
            {
                result.Device = mapContext.DeviceMap.Map(source.Device, options);
                result.Board = mapContext.BoardMap.Map(source.Board, options);
                result.Channel = mapContext.ChannelMap.Map(source.Channel, options);
            }
            if (options.MapCollections)
            {
            }

            return result;
        }

        public override DeviceLog ReverseMapCore(DeviceLogDto source, MapOptions options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new DeviceLog();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.Time = source.Time.ToUtc();
                result.RecordType = source.RecordType;
                result.Name = source.Name;
                if (source.Payload != null)
                    result.Payload = JsonConvert.SerializeObject(source.Payload);
                result.State = source.State;
                result.ErrorMessage = source.ErrorMessage;
                result.DeviceId = source.DeviceId;
                result.BoardId = source.BoardId;
                result.ChannelId = source.ChannelId;
            }
            if (options.MapObjects)
            {
                if (source.DeviceId == null)
                    result.Device = mapContext.DeviceMap.ReverseMap(source.Device, options);
                if (source.BoardId == null)
                    result.Board = mapContext.BoardMap.ReverseMap(source.Board, options);
                if (source.ChannelId == null)
                    result.Channel = mapContext.ChannelMap.ReverseMap(source.Channel, options);
            }
            if (options.MapCollections)
            {
            }

            return result;
        }

        public override void MapCore(DeviceLog source, DeviceLog destination, MapOptions options = null)
        {
            if (source == null || destination == null)
                return;

            options = options ?? new MapOptions();

            destination.Id = source.Id;
            if (options.MapProperties)
            {
                destination.Time = source.Time;
                destination.RecordType = source.RecordType;
                destination.Name = source.Name;
                destination.Payload = JsonHelper.NormalizeSafe(source.Payload);
                destination.State = source.State;
                destination.ErrorMessage = source.ErrorMessage;
                destination.DeviceId = source.DeviceId;
                destination.BoardId = source.BoardId;
                destination.ChannelId = source.ChannelId;
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
