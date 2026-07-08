using Lockium.Mappings;
using Lockium.Mappings.Devices;
using Lockium.Mappings.Lockers;
using Lockium.Mappings.Reservations;
using Lockium.Mappings.Transactions;
using Lockium.Mappings.Orders;
using Lockium.Mappings.Billings;

namespace Lockium.Transactions.Helpers
{
    static class StartupHelper
    {
        public static void AddMapping(this WebApplicationBuilder source)
        {
            var services = source.Services;

            services.AddScoped<DbMapContext>();

            services.AddScoped<UserMap>();
            services.AddScoped<RoleMap>();
            services.AddScoped<UserRoleMap>();
            services.AddScoped<DeviceMap>();
            services.AddScoped<ChannelMap>();
            services.AddScoped<DeviceLogMap>();
            services.AddScoped<LockerMap>();
            services.AddScoped<CellMap>();
            services.AddScoped<ReservationMap>();
            services.AddScoped<OrderMap>();
            services.AddScoped<TransactionMap>();
            services.AddScoped<BillingMap>();
        }

        public static void AddServices(this WebApplicationBuilder source)
        {
            var services = source.Services;


        }

        public static void AddProviders(this WebApplicationBuilder source)
        {
            var services = source.Services;

        }
    }
}
