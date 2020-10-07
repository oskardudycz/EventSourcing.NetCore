using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shipments.Packages;
using Shipments.Storage;

namespace Shipments
{
    public static class Config
    {
        public static IServiceCollection AddShipmentsModule(this IServiceCollection services, IConfiguration config)
        {
            return services
                .AddEntityFramework(config)
                .AddPackages();
        }

        private static IServiceCollection AddEntityFramework(this IServiceCollection services, IConfiguration config)
        {
            return services.AddDbContext<ShipmentsDbContext>(
                options => options.UseNpgsql(config.GetConnectionString("ShipmentsDatabase")));
        }

        public static void ConfigureShipments(this IServiceProvider serviceProvider)
        {
            // Kids, don't try this at production
            serviceProvider.GetService<ShipmentsDbContext>().Database.Migrate();
        }
    }
}
