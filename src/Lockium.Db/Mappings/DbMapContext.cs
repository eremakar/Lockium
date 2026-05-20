using Lockium.Mappings.Devices;
using Lockium.Mappings.Reservations;
using Lockium.Mappings.Orders;

namespace Lockium.Mappings
{
    public partial class DbMapContext
    {
        public UserMap UserMap { get; }
        public RoleMap RoleMap { get; }
        public UserRoleMap UserRoleMap { get; }
        public DeviceMap DeviceMap { get; }
        public ChannelMap ChannelMap { get; }
        public ReservationMap ReservationMap { get; }
        public OrderMap OrderMap { get; }

        public DbMapContext()
        {
            UserMap = new UserMap(this);
            RoleMap = new RoleMap(this);
            UserRoleMap = new UserRoleMap(this);
            DeviceMap = new DeviceMap(this);
            ChannelMap = new ChannelMap(this);
            ReservationMap = new ReservationMap(this);
            OrderMap = new OrderMap(this);
        }
    }
}
