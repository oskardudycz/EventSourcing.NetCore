using System;
using ECommerce.Pricing;
using ECommerce.ShoppingCarts;
using ECommerce.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce;

public static class Configuration
{
    public static IServiceCollection AddECommerceModule(this IServiceCollection services, IConfiguration config) =>
        services
            .AddShoppingCartsModule()
            .AddPricingModule()
            .AddDbContext<ECommerceDbContext>(
                options =>
                {
                    var connectionString = config.GetConnectionString("ECommerceDB");
                    var schemaName = Environment.GetEnvironmentVariable("SchemaName") ?? "simple_esdb_ecommerce";

                    options.UseNpgsql(
                        $"{connectionString};searchpath = {schemaName.ToLower()}",
                        x => x.MigrationsHistoryTable("__EFMigrationsHistory", schemaName.ToLower()));
                })
            .AddSingleton<Func<Guid>>(Guid.NewGuid);

    public static void UseECommerceModule(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
        dbContext.Database.Migrate();
    }
}
