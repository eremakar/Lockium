using Data.Mapping;
using Data.Repository.Helpers;
using Lockium.Data.LockiumDb.Entities.Transactions;
using Lockium.Models.Dtos;
using Lockium.Models.Dtos.Transactions;

namespace Lockium.Mappings.Transactions
{
    /// <summary>
    /// Транзакция, создаётся при брони или заказе
    /// </summary>
    public partial class TransactionMap : MapBase2<Transaction, TransactionDto, MapOptions>
    {
        private readonly DbMapContext mapContext;

        public TransactionMap(DbMapContext mapContext)
        {
            this.mapContext = mapContext;
        }

        public override TransactionDto MapCore(Transaction source, MapOptions? options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new TransactionDto();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.State = source.State;
                result.SourceType = source.SourceType;
                result.CreatedTime = source.CreatedTime;
                result.ClientId = source.ClientId;
                result.ReservationId = source.ReservationId;
                result.OrderId = source.OrderId;
            }
            if (options.MapObjects)
            {
                if (source.Client != null)
                {
                    result.Client = new UserDto
                    {
                        UserName = source.Client.UserName,
                    };
                }

                result.Reservation = mapContext.ReservationMap.Map(source.Reservation, options);
                result.Order = mapContext.OrderMap.Map(source.Order, options);
            }
            if (options.MapCollections)
            {
            }

            return result;
        }

        public override Transaction ReverseMapCore(TransactionDto source, MapOptions options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new Transaction();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.State = source.State;
                result.SourceType = source.SourceType;
                result.CreatedTime = source.CreatedTime.ToUtc();
                result.ClientId = source.ClientId;
                result.ReservationId = source.ReservationId;
                result.OrderId = source.OrderId;
            }
            if (options.MapObjects)
            {
                if (source.ClientId == null)
                    result.Client = mapContext.UserMap.ReverseMap(source.Client, options);
                if (source.ReservationId == null)
                    result.Reservation = mapContext.ReservationMap.ReverseMap(source.Reservation, options);
                if (source.OrderId == null)
                    result.Order = mapContext.OrderMap.ReverseMap(source.Order, options);
            }
            if (options.MapCollections)
            {
            }

            return result;
        }

        public override void MapCore(Transaction source, Transaction destination, MapOptions options = null)
        {
            if (source == null || destination == null)
                return;

            options = options ?? new MapOptions();

            destination.Id = source.Id;
            if (options.MapProperties)
            {
                destination.State = source.State;
                destination.SourceType = source.SourceType;
                destination.CreatedTime = source.CreatedTime;
                destination.ClientId = source.ClientId;
                destination.ReservationId = source.ReservationId;
                destination.OrderId = source.OrderId;
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
