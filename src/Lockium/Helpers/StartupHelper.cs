using Lockium.Mappings;
using Lockium.Services;

namespace Lockium.Helpers
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
        }

        public static void AddServices(this WebApplicationBuilder source)
        {
            var services = source.Services;

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();

        }

        public static void AddProviders(this WebApplicationBuilder source)
        {
            var services = source.Services;

        }
    }
}
