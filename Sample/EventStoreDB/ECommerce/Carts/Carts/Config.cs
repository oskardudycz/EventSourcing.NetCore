using Carts.Carts;
using Core.EventStoreDB;
using Core.Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Carts;

public static class Config
{
    public static void AddCartsModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddEventStoreDB(config);
        // Document Part used for projections
        services.AddMarten(config, configKey: "ReadModel_Marten");
        services.AddCarts();
    }
}
