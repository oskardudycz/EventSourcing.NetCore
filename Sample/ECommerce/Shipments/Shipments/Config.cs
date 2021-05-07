using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shipments.Packages;
using Shipments.Products;
using Shipments.Storage;

namespace Shipments
{
    public static class Config
    {
        public static IServiceCollection AddShipmentsModule(this IServiceCollection services, IConfiguration config)
        {
            return services
                .AddEntityFramework(config)
                .AddPackages()
                .AddProducts();
        }

        private static IServiceCollection AddEntityFramework(this IServiceCollection services, IConfiguration config)
        {
            return services.AddDbContext<ShipmentsDbContext>(
                options => options.UseNpgsql(config.GetConnectionString("ShipmentsDatabase")));
        }

        public static void ConfigureShipmentsModule(this IServiceProvider serviceProvider)
        {
            // Kids, don't try this at production
            serviceProvider.GetRequiredService<ShipmentsDbContext>().Database.Migrate();
        }
    }
}
