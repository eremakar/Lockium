using Data.Mapping;
using Lockium.Data.LockiumDb.Entities.Devices;
using Lockium.Models.Dtos.Devices;

namespace Lockium.Mappings.Devices
{
    public partial class IRChannelMap : MapBase2<IRChannel, IRChannelDto, MapOptions>
    {
        private readonly DbMapContext mapContext;

        public IRChannelMap(DbMapContext mapContext)
        {
            this.mapContext = mapContext;
        }

        public override IRChannelDto MapCore(IRChannel source, MapOptions? options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new IRChannelDto();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.Number = source.Number;
                result.State = source.State;
                result.BoardId = source.BoardId;
            }
            if (options.MapObjects)
            {
                result.Board = mapContext.BoardMap.Map(source.Board, options);
            }
            if (options.MapCollections)
            {
            }

            return result;
        }

        public override IRChannel ReverseMapCore(IRChannelDto source, MapOptions options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new IRChannel();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.Number = source.Number;
                result.State = source.State;
                result.BoardId = source.BoardId;
            }
            if (options.MapObjects)
            {
                if (source.BoardId == null)
                    result.Board = mapContext.BoardMap.ReverseMap(source.Board, options);
            }
            if (options.MapCollections)
            {
            }

            return result;
        }

        public override void MapCore(IRChannel source, IRChannel destination, MapOptions options = null)
        {
            if (source == null || destination == null)
                return;

            options = options ?? new MapOptions();

            destination.Id = source.Id;
            if (options.MapProperties)
            {
                destination.Number = source.Number;
                destination.State = source.State;
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
