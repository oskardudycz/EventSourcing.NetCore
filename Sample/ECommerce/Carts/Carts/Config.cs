using Carts.ShoppingCarts;
using Core.Marten;
using JasperFx.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Carts;

public static class Config
{
    public static IServiceCollection AddCartsModule(this IServiceCollection services, IConfiguration config) =>
        services
            .AddMarten(config, options =>
            {
                options.Projections.DaemonLockId = 222222;
                options.ConfigureCarts();
                options.DisableNpgsqlLogging = true;
                options.Events.StreamIdentity = StreamIdentity.AsGuid;
            }).UseNpgsqlDataSource()
            .Services
            .AddCarts();
}
