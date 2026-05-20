using Lockium.Data.LockiumDb.Entities;
using Lockium.Data.LockiumDb.Entities.Devices;
using Lockium.Data.LockiumDb.Entities.Reservations;
using Lockium.Data.LockiumDb.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lockium.Data.LockiumDb.DatabaseContext
{
    public class UsersConfiguration : IEntityTypeConfiguration<User>
    {
        public bool IsInMemoryDb { get; set; }

        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Id);

            if (!IsInMemoryDb)
            {
                builder.Property(_ => _.Avatar).HasColumnType("jsonb");
            }
            else
            {
                builder.Ignore(_ => _.Avatar);
            }
        }
    }

    public class RolesConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }

    public class UserRolesConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }

    public class DevicesConfiguration : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }

    public class ChannelsConfiguration : IEntityTypeConfiguration<Channel>
    {
        public bool IsInMemoryDb { get; set; }

        public void Configure(EntityTypeBuilder<Channel> builder)
        {
            builder.HasKey(x => x.Id);

            if (!IsInMemoryDb)
            {
                builder.Property(_ => _.Attributes).HasColumnType("jsonb");
            }
            else
            {
                builder.Ignore(_ => _.Attributes);
            }
        }
    }

    public class ReservationsConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }

    public class OrdersConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}
