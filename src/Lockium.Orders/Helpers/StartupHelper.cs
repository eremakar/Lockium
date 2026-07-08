using Lockium.Mappings;
using Lockium.Mappings.Devices;
using Lockium.Mappings.Reservations;
using Lockium.Mappings.Orders;
using Lockium.Workflows;

namespace Lockium.Orders.Helpers
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
            services.AddScoped<ReservationMap>();
            services.AddScoped<OrderMap>();
        }

        public static void AddServices(this WebApplicationBuilder source)
        {
            var services = source.Services;

            services.AddLockiumOrderWorkflows();
        }

        public static void AddProviders(this WebApplicationBuilder source)
        {
            var services = source.Services;

        }
    }
}
