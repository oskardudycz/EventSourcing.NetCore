using System.Runtime.CompilerServices;
using Carts.Carts;
using Core.Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("Marten.Generated")]

namespace Carts;

public static class Config
{
    public static void AddCartsModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddMarten(config, options =>
        {
            options.ConfigureCarts();
        });
        services.AddCarts();
    }
}
