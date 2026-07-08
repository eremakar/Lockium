using Lockium.Mappings;
using Lockium.Mappings.Devices;
using Lockium.Mappings.Lockers;
using Lockium.Mappings.Reservations;
using Lockium.Mappings.Orders;
using Lockium.Workflows;
using Lockium.Client.Api.Models;
using Lockium.Client.Api.Services;
using Lockium.Client.Api.Services.Orders;

namespace Lockium.Client.Api.Helpers
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
            services.AddScoped<LockerMap>();
            services.AddScoped<CellMap>();
            services.AddScoped<ReservationMap>();
            services.AddScoped<OrderMap>();
        }

        public static void AddServices(this WebApplicationBuilder source)
        {
            var services = source.Services;

            services.AddLockiumOrderWorkflows();
            services.AddLockiumReservationWorkflows();

            services.Configure<LockiumOptions>(source.Configuration.GetSection(LockiumOptions.SectionName));
            services.AddHttpContextAccessor();
            services.AddHttpClient("Lockium", (sp, client) =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<LockiumOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            });
            services.AddScoped<ILockiumGateway, LockiumGateway>();
            services.AddScoped<IOrderCreateService, OrderCreateService>();
            services.AddScoped<IOrderOperationsService, OrderOperationsService>();
        }

        public static void AddProviders(this WebApplicationBuilder source)
        {
        }
    }
}
