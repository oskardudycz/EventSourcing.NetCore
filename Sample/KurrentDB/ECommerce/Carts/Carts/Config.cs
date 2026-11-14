using Carts.ShoppingCarts;
using Core.KurrentDB;
using Core.KurrentDB.Subscriptions.Checkpoints.Postgres;
using Core.Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Carts;

public static class Config
{
    public static IServiceCollection AddCartsModule(this IServiceCollection services, IConfiguration config) =>
        services
            // Document Part used for projections
            .AddMarten(config, configKey: "ReadModel_Marten", disableAsyncDaemon: true)
            .Services
            .AddCarts()
            .AddKurrentDB(config);
}
