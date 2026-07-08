using Data.Mapping;
using Data.Repository.Helpers;
using Lockium.Data.LockiumDb.Entities.Billings;
using Lockium.Models.Dtos.Billings;

namespace Lockium.Mappings.Billings
{
    /// <summary>
    /// Данные по времени брони и заказа
    /// </summary>
    public partial class BillingMap : MapBase2<Billing, BillingDto, MapOptions>
    {
        private readonly DbMapContext mapContext;

        public BillingMap(DbMapContext mapContext)
        {
            this.mapContext = mapContext;
        }

        public override BillingDto MapCore(Billing source, MapOptions? options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new BillingDto();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.StartTime = source.StartTime;
                result.EndTime = source.EndTime;
                result.Amount = source.Amount;
                result.Duration = source.Duration;
                result.TransactionId = source.TransactionId;
                result.ReservationId = source.ReservationId;
                result.OrderId = source.OrderId;
            }
            if (options.MapObjects)
            {
                result.Transaction = mapContext.TransactionMap.Map(source.Transaction, options);
                result.Reservation = mapContext.ReservationMap.Map(source.Reservation, options);
                result.Order = mapContext.OrderMap.Map(source.Order, options);
            }
            if (options.MapCollections)
            {
            }

            return result;
        }

        public override Billing ReverseMapCore(BillingDto source, MapOptions options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new Billing();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.StartTime = source.StartTime.ToUtc();
                result.EndTime = source.EndTime.ToUtc();
                result.Amount = source.Amount;
                result.Duration = source.Duration;
                result.TransactionId = source.TransactionId;
                result.ReservationId = source.ReservationId;
                result.OrderId = source.OrderId;
            }
            if (options.MapObjects)
            {
                if (source.TransactionId == null)
                    result.Transaction = mapContext.TransactionMap.ReverseMap(source.Transaction, options);
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

        public override void MapCore(Billing source, Billing destination, MapOptions options = null)
        {
            if (source == null || destination == null)
                return;

            options = options ?? new MapOptions();

            destination.Id = source.Id;
            if (options.MapProperties)
            {
                destination.StartTime = source.StartTime;
                destination.EndTime = source.EndTime;
                destination.Amount = source.Amount;
                destination.Duration = source.Duration;
                destination.TransactionId = source.TransactionId;
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
