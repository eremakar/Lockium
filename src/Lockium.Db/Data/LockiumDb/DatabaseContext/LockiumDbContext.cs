using Data.Repository;
using Lockium.Data.LockiumDb.Entities;
using Lockium.Data.LockiumDb.Entities.Devices;
using Lockium.Data.LockiumDb.Entities.Lockers;
using Lockium.Data.LockiumDb.Entities.Reservations;
using Lockium.Data.LockiumDb.Entities.Orders;
using Lockium.Data.LockiumDb.Entities.Transactions;
using Lockium.Data.LockiumDb.Entities.Billings;
using Microsoft.EntityFrameworkCore;

namespace Lockium.Data.LockiumDb.DatabaseContext
{
    public partial class LockiumDbContext : DbContext
    {
        public DbSet<User>? Users { get; set; }
        public DbSet<Role>? Roles { get; set; }
        public DbSet<UserRole>? UserRoles { get; set; }
        public DbSet<Device>? Devices { get; set; }
        public DbSet<Board>? Boards { get; set; }
        public DbSet<Channel>? Channels { get; set; }
        public DbSet<IRChannel>? IRChannels { get; set; }
        public DbSet<DeviceLog>? DeviceLogs { get; set; }
        public DbSet<Locker>? Lockers { get; set; }
        public DbSet<Cell>? Cells { get; set; }
        public DbSet<Reservation>? Reservations { get; set; }
        public DbSet<Order>? Orders { get; set; }
        public DbSet<Transaction>? Transactions { get; set; }
        public DbSet<Billing>? Billings { get; set; }

        public LockiumDbContext(DbContextOptions<LockiumDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UsersConfiguration { IsInMemoryDb = this.IsInMemoryDb() });
            modelBuilder.ApplyConfiguration(new RolesConfiguration());
            modelBuilder.ApplyConfiguration(new UserRolesConfiguration());
            modelBuilder.ApplyConfiguration(new DevicesConfiguration());
            modelBuilder.ApplyConfiguration(new BoardsConfiguration());
            modelBuilder.ApplyConfiguration(new ChannelsConfiguration { IsInMemoryDb = this.IsInMemoryDb() });
            modelBuilder.ApplyConfiguration(new IRChannelsConfiguration());
            modelBuilder.ApplyConfiguration(new DeviceLogsConfiguration { IsInMemoryDb = this.IsInMemoryDb() });
            modelBuilder.ApplyConfiguration(new LockersConfiguration());
            modelBuilder.ApplyConfiguration(new CellsConfiguration { IsInMemoryDb = this.IsInMemoryDb() });
            modelBuilder.ApplyConfiguration(new ReservationsConfiguration());
            modelBuilder.ApplyConfiguration(new OrdersConfiguration());
            modelBuilder.ApplyConfiguration(new TransactionsConfiguration());
            modelBuilder.ApplyConfiguration(new BillingsConfiguration());
        }
    }
}
