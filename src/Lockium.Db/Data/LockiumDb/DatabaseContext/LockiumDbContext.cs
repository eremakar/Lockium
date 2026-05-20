using Data.Repository;
using Lockium.Data.LockiumDb.Entities;
using Lockium.Data.LockiumDb.Entities.Devices;
using Lockium.Data.LockiumDb.Entities.Reservations;
using Lockium.Data.LockiumDb.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace Lockium.Data.LockiumDb.DatabaseContext
{
    public partial class LockiumDbContext : DbContext
    {
        public DbSet<User>? Users { get; set; }
        public DbSet<Role>? Roles { get; set; }
        public DbSet<UserRole>? UserRoles { get; set; }
        public DbSet<Device>? Devices { get; set; }
        public DbSet<Channel>? Channels { get; set; }
        public DbSet<Reservation>? Reservations { get; set; }
        public DbSet<Order>? Orders { get; set; }

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
            modelBuilder.ApplyConfiguration(new ChannelsConfiguration { IsInMemoryDb = this.IsInMemoryDb() });
            modelBuilder.ApplyConfiguration(new ReservationsConfiguration());
            modelBuilder.ApplyConfiguration(new OrdersConfiguration());
        }
    }
}
