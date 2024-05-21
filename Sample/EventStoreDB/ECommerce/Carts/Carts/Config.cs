using Carts.ShoppingCarts;
using Core.EventStoreDB;
using Core.EventStoreDB.Subscriptions.Checkpoints.Postgres;
using Core.Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Carts;

public static class Config
{
    public static IServiceCollection AddCartsModule(this IServiceCollection services, IConfiguration config) =>
        services
            // Document Part used for projections
            .AddMarten(config, configKey: "ReadModel_Marten")
            .Services
            .AddCarts()
            .AddEventStoreDB(config);
}
