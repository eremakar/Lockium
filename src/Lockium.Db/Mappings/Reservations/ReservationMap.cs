using Data.Mapping;
using Data.Repository.Helpers;
using Lockium.Data.LockiumDb.Entities.Reservations;
using Lockium.Models.Dtos;
using Lockium.Models.Dtos.Reservations;

namespace Lockium.Mappings.Reservations
{
    public partial class ReservationMap : MapBase2<Reservation, ReservationDto, MapOptions>
    {
        private readonly DbMapContext mapContext;

        public ReservationMap(DbMapContext mapContext)
        {
            this.mapContext = mapContext;
        }

        public override ReservationDto MapCore(Reservation source, MapOptions? options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new ReservationDto();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.State = source.State;
                result.CreatedTime = source.CreatedTime;
                result.ClientId = source.ClientId;
                result.ChannelId = source.ChannelId;
            }
            if (options.MapObjects)
            {
                if (source.Client != null)
                {
                    result.Client = new UserDto
                    {
                        Id = source.Client.Id,
                        UserName = source.Client.UserName,
                    };
                }

                result.Channel = mapContext.ChannelMap.Map(source.Channel, options);
            }
            if (options.MapCollections)
            {
            }

            return result;
        }

        public override Reservation ReverseMapCore(ReservationDto source, MapOptions options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new Reservation();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.State = source.State;
                result.CreatedTime = source.CreatedTime.ToUtc();
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

        public override void MapCore(Reservation source, Reservation destination, MapOptions options = null)
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
