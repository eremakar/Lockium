using Lockium.Data.LockiumDb.Entities;
using Lockium.Data.LockiumDb.Entities.Devices;
using Lockium.Data.LockiumDb.Entities.Lockers;
using Lockium.Data.LockiumDb.Entities.Reservations;
using Lockium.Data.LockiumDb.Entities.Orders;
using Lockium.Data.LockiumDb.Entities.Transactions;
using Lockium.Data.LockiumDb.Entities.Billings;
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

    public class BoardsConfiguration : IEntityTypeConfiguration<Board>
    {
        public void Configure(EntityTypeBuilder<Board> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.Up)
                .WithMany();
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

    public class IRChannelsConfiguration : IEntityTypeConfiguration<IRChannel>
    {
        public void Configure(EntityTypeBuilder<IRChannel> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }

    public class DeviceLogsConfiguration : IEntityTypeConfiguration<DeviceLog>
    {
        public bool IsInMemoryDb { get; set; }

        public void Configure(EntityTypeBuilder<DeviceLog> builder)
        {
            builder.HasKey(x => x.Id);

            if (!IsInMemoryDb)
            {
                builder.Property(_ => _.Payload).HasColumnType("jsonb");
            }
            else
            {
                builder.Ignore(_ => _.Payload);
            }
        }
    }

    public class LockersConfiguration : IEntityTypeConfiguration<Locker>
    {
        public void Configure(EntityTypeBuilder<Locker> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }

    public class CellsConfiguration : IEntityTypeConfiguration<Cell>
    {
        public bool IsInMemoryDb { get; set; }

        public void Configure(EntityTypeBuilder<Cell> builder)
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

    public class TransactionsConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }

    public class BillingsConfiguration : IEntityTypeConfiguration<Billing>
    {
        public void Configure(EntityTypeBuilder<Billing> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}
