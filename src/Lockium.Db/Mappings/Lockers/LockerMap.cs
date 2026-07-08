using Data.Mapping;
using Lockium.Data.LockiumDb.Entities.Lockers;
using Lockium.Models.Dtos.Lockers;

namespace Lockium.Mappings.Lockers
{
    /// <summary>
    /// Шкаф (постамат и т.п.)
    /// </summary>
    public partial class LockerMap : MapBase2<Locker, LockerDto, MapOptions>
    {
        private readonly DbMapContext mapContext;

        public LockerMap(DbMapContext mapContext)
        {
            this.mapContext = mapContext;
        }

        public override LockerDto MapCore(Locker source, MapOptions? options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new LockerDto();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.Name = source.Name;
                result.Address = source.Address;
                result.Type = source.Type;
            }
            if (options.MapObjects)
            {
            }
            if (options.MapCollections)
            {
                result.Cells = mapContext.CellMap.Map(source.Cells, options);
            }

            return result;
        }

        public override Locker ReverseMapCore(LockerDto source, MapOptions options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new Locker();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.Name = source.Name;
                result.Address = source.Address;
                result.Type = source.Type;
            }
            if (options.MapObjects)
            {
            }
            if (options.MapCollections)
            {
                result.Cells = mapContext.CellMap.ReverseMap(source.Cells, options);
            }

            return result;
        }

        public override void MapCore(Locker source, Locker destination, MapOptions options = null)
        {
            if (source == null || destination == null)
                return;

            options = options ?? new MapOptions();

            destination.Id = source.Id;
            if (options.MapProperties)
            {
                destination.Name = source.Name;
                destination.Address = source.Address;
                destination.Type = source.Type;
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
