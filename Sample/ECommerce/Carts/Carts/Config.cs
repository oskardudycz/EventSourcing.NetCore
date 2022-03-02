using System.Runtime.CompilerServices;
using Carts.ShoppingCarts;
using Core.Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("Marten.Generated")]

namespace Carts;

public static class Config
{
    public static IServiceCollection AddCartsModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddMarten(config, options =>
        {
            options.ConfigureCarts();
        });
        return services.AddCarts();
    }
}
