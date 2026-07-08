using Data.Mapping;
using Data.Repository.Helpers;
using Lockium.Data.LockiumDb.Entities.Orders;
using Lockium.Models.Dtos.Orders;
using Newtonsoft.Json;

namespace Lockium.Mappings.Orders
{
    public partial class OrderMap : MapBase2<Order, OrderDto, MapOptions>
    {
        private readonly DbMapContext mapContext;

        public OrderMap(DbMapContext mapContext)
        {
            this.mapContext = mapContext;
        }

        private static object? DeserializeJsonObject(string? source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return null;

            return JsonConvert.DeserializeObject(JsonHelper.NormalizeSafe(source));
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
                result.CreatedTime = source.CreatedTime;
                result.PinCode = source.PinCode;
                result.DepositOpened = source.DepositOpened;
                result.PickupOpened = source.PickupOpened;
                result.TrackingNumber = source.TrackingNumber;
                result.ExpiresAt = source.ExpiresAt;
                result.Recipient = DeserializeJsonObject(source.Recipient);
                result.ClientId = source.ClientId;
                result.LockerId = source.LockerId;
                result.CellId = source.CellId;
                result.ChannelId = source.ChannelId;
            }
            if (options.MapObjects)
            {
                if (source.Client != null)
                {
                    result.Client = new Lockium.Models.Dtos.UserDto
                    {
                        Id = source.Client.Id,
                        UserName = source.Client.UserName,
                    };
                }

                result.Locker = mapContext.LockerMap.Map(source.Locker, options);
                result.Cell = mapContext.CellMap.Map(source.Cell, options);
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
                result.CreatedTime = source.CreatedTime.ToUtc();
                result.PinCode = source.PinCode;
                result.DepositOpened = source.DepositOpened;
                result.PickupOpened = source.PickupOpened;
                result.TrackingNumber = source.TrackingNumber;
                result.ExpiresAt = source.ExpiresAt.ToUtc();
                if (source.Recipient != null)
                    result.Recipient = JsonConvert.SerializeObject(source.Recipient);
                result.ClientId = source.ClientId;
                result.LockerId = source.LockerId;
                result.CellId = source.CellId;
                result.ChannelId = source.ChannelId;
            }
            if (options.MapObjects)
            {
                if (source.ClientId == null)
                    result.Client = mapContext.UserMap.ReverseMap(source.Client, options);
                if (source.LockerId == null)
                    result.Locker = mapContext.LockerMap.ReverseMap(source.Locker, options);
                if (source.CellId == null)
                    result.Cell = mapContext.CellMap.ReverseMap(source.Cell, options);
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
                destination.CreatedTime = source.CreatedTime;
                destination.PinCode = source.PinCode;
                destination.DepositOpened = source.DepositOpened;
                destination.PickupOpened = source.PickupOpened;
                destination.TrackingNumber = source.TrackingNumber;
                destination.ExpiresAt = source.ExpiresAt;
                destination.Recipient = JsonHelper.NormalizeSafe(source.Recipient);
                destination.ClientId = source.ClientId;
                destination.LockerId = source.LockerId;
                destination.CellId = source.CellId;
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
