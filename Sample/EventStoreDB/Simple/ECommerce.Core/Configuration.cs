using ECommerce.Core.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Core
{
    public static class Configuration
    {
        public static IServiceCollection AddCoreServices(
            this IServiceCollection services,
            IConfiguration configuration
        ) =>
            services
                .AddEventBus()
                .AddEventStoreDB(configuration);
    }
}
