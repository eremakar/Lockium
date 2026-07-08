using Lockium.Mappings.Devices;
using Lockium.Mappings.Lockers;
using Lockium.Mappings.Reservations;
using Lockium.Mappings.Orders;
using Lockium.Mappings.Transactions;
using Lockium.Mappings.Billings;

namespace Lockium.Mappings
{
    public partial class DbMapContext
    {
        public UserMap UserMap { get; }
        public RoleMap RoleMap { get; }
        public UserRoleMap UserRoleMap { get; }
        public DeviceMap DeviceMap { get; }
        public BoardMap BoardMap { get; }
        public ChannelMap ChannelMap { get; }
        public IRChannelMap IRChannelMap { get; }
        public DeviceLogMap DeviceLogMap { get; }
        public LockerMap LockerMap { get; }
        public CellMap CellMap { get; }
        public ReservationMap ReservationMap { get; }
        public OrderMap OrderMap { get; }
        public TransactionMap TransactionMap { get; }
        public BillingMap BillingMap { get; }

        public DbMapContext()
        {
            UserMap = new UserMap(this);
            RoleMap = new RoleMap(this);
            UserRoleMap = new UserRoleMap(this);
            DeviceMap = new DeviceMap(this);
            BoardMap = new BoardMap(this);
            ChannelMap = new ChannelMap(this);
            IRChannelMap = new IRChannelMap(this);
            DeviceLogMap = new DeviceLogMap(this);
            LockerMap = new LockerMap(this);
            CellMap = new CellMap(this);
            ReservationMap = new ReservationMap(this);
            OrderMap = new OrderMap(this);
            TransactionMap = new TransactionMap(this);
            BillingMap = new BillingMap(this);
        }
    }
}
