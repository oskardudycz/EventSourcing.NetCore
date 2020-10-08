using Carts.Carts;
using Core.Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Carts
{
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
}
