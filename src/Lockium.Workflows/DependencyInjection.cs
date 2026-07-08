using Lockium.Workflows.Orders;
using Lockium.Workflows.Reservations;
using Lockium.Workflows.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lockium.Workflows
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddLockiumOrderWorkflows(this IServiceCollection services)
        {
            services.AddScoped<IBillingCalculator, BillingCalculator>();
            services.AddScoped<Orders.IMicroserviceStateWorkflow, Orders.MicroserviceStateWorkflow>();
            return services;
        }

        public static IServiceCollection AddLockiumReservationWorkflows(this IServiceCollection services)
        {
            services.AddScoped<Reservations.IMicroserviceStateWorkflow, Reservations.MicroserviceStateWorkflow>();
            return services;
        }
    }
}
