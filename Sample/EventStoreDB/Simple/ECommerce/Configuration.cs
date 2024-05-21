using Core.Configuration;
using Core.EntityFramework.Projections;
using Core.EventStoreDB.Subscriptions.Checkpoints;
using ECommerce.Pricing;
using ECommerce.ShoppingCarts;
using ECommerce.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace ECommerce;

public static class Configuration
{
    public static IServiceCollection AddECommerceModule(this IServiceCollection services, IConfiguration config)
    {
        var schemaName = // Environment.GetEnvironmentVariable("SchemaName") ??
            "simple_esdb_ecommerce";
        var connectionString = $"{config.GetConnectionString("ECommerceDB")};searchpath = {schemaName.ToLower()}";

        return services
            .AddNpgsqlDataSource(connectionString)
            .AddScoped<NpgsqlConnection>(_ =>
            {
                var connection = new NpgsqlConnection(connectionString);
                connection.Open();
                return connection;
            })
            .AddScoped<NpgsqlTransaction>(sp =>
            {
                var connection = sp.GetRequiredService<NpgsqlConnection>();
                return connection.BeginTransaction();
            })
            .AddEntityFrameworkProjections<ECommerceDbContext>()
            .AddShoppingCartsModule()
            .AddPricingModule()
            .AddDbContext<ECommerceDbContext>(
                (sp, options) =>
                {
                    var connection = sp.GetRequiredService<NpgsqlConnection>();

                    options.UseNpgsql(connection,
                        x => x.MigrationsHistoryTable("__EFMigrationsHistory", schemaName.ToLower()));
                })
            .AddSingleton<Func<Guid>>(Guid.NewGuid);
    }

    public static void UseECommerceModule(this IServiceProvider serviceProvider)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        if (environment != "Development")
            return;

        using var scope = serviceProvider.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
        dbContext.Database.Migrate();
    }
}
