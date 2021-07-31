using System;
using Microsoft.AspNetCore.Builder;
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
                options => options.UseNpgsql("name=ConnectionStrings:ShipmentsDatabase"));
        }

        public static void ConfigureShipmentsModule(this IApplicationBuilder app)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            if (environment == "Development")
            {
                using var serviceScope = app.ApplicationServices.CreateScope();
                serviceScope.ServiceProvider.GetRequiredService<ShipmentsDbContext>().Database.Migrate();
            }
        }
    }
}
