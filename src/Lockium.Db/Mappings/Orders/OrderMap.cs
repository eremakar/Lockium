using Data.Mapping;
using Lockium.Data.LockiumDb.Entities.Orders;
using Lockium.Models.Dtos.Orders;

namespace Lockium.Mappings.Orders
{
    public partial class OrderMap : MapBase2<Order, OrderDto, MapOptions>
    {
        private readonly DbMapContext mapContext;

        public OrderMap(DbMapContext mapContext)
        {
            this.mapContext = mapContext;
        }

        public override OrderDto MapCore(Order source, MapOptions? options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new OrderDto();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.State = source.State;
                result.ClientId = source.ClientId;
                result.ChannelId = source.ChannelId;
            }
            if (options.MapObjects)
            {
                result.Client = mapContext.UserMap.Map(source.Client, options);
                result.Channel = mapContext.ChannelMap.Map(source.Channel, options);
            }
            if (options.MapCollections)
            {
            }

            return result;
        }

        public override Order ReverseMapCore(OrderDto source, MapOptions options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new Order();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.State = source.State;
                result.ClientId = source.ClientId;
                result.ChannelId = source.ChannelId;
            }
            if (options.MapObjects)
            {
                if (source.ClientId == null)
                    result.Client = mapContext.UserMap.ReverseMap(source.Client, options);
                if (source.ChannelId == null)
                    result.Channel = mapContext.ChannelMap.ReverseMap(source.Channel, options);
            }
            if (options.MapCollections)
            {
            }

            return result;
        }

        public override void MapCore(Order source, Order destination, MapOptions options = null)
        {
            if (source == null || destination == null)
                return;

            options = options ?? new MapOptions();

            destination.Id = source.Id;
            if (options.MapProperties)
            {
                destination.State = source.State;
                destination.ClientId = source.ClientId;
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
