using Data.Mapping;
using Lockium.Data.LockiumDb.Entities.Lockers;
using Lockium.Models.Dtos.Lockers;
using Newtonsoft.Json;
using Data.Repository.Helpers;

namespace Lockium.Mappings.Lockers
{
    /// <summary>
    /// Ячейка шкафа
    /// </summary>
    public partial class CellMap : MapBase2<Cell, CellDto, MapOptions>
    {
        private readonly DbMapContext mapContext;

        public CellMap(DbMapContext mapContext)
        {
            this.mapContext = mapContext;
        }

        public override CellDto MapCore(Cell source, MapOptions? options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new CellDto();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.Number = source.Number;
                result.State = source.State;
                result.Attributes = source.Attributes;
                result.LockerId = source.LockerId;
                result.ChannelId = source.ChannelId;
            }
            if (options.MapObjects)
            {
                result.Locker = mapContext.LockerMap.Map(source.Locker, options);
                result.Channel = mapContext.ChannelMap.Map(source.Channel, options);
            }
            if (options.MapCollections)
            {
            }

            return result;
        }

        public override Cell ReverseMapCore(CellDto source, MapOptions options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new Cell();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.Number = source.Number;
                result.State = source.State;
                if (source.Attributes != null)
                    result.Attributes = JsonConvert.SerializeObject(source.Attributes);
                result.LockerId = source.LockerId;
                result.ChannelId = source.ChannelId;
            }
            if (options.MapObjects)
            {
                if (source.LockerId == null)
                    result.Locker = mapContext.LockerMap.ReverseMap(source.Locker, options);
                if (source.ChannelId == null)
                    result.Channel = mapContext.ChannelMap.ReverseMap(source.Channel, options);
            }
            if (options.MapCollections)
            {
            }

            return result;
        }

        public override void MapCore(Cell source, Cell destination, MapOptions options = null)
        {
            if (source == null || destination == null)
                return;

            options = options ?? new MapOptions();

            destination.Id = source.Id;
            if (options.MapProperties)
            {
                destination.Number = source.Number;
                destination.State = source.State;
                destination.Attributes = JsonHelper.NormalizeSafe(source.Attributes);
                destination.LockerId = source.LockerId;
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
