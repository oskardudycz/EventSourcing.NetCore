using Carts.Carts;
using Core.EventStoreDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Carts
{
    public static class Config
    {
        public static void AddCartsModule(this IServiceCollection services, IConfiguration config)
        {
            services.AddEventStoreDB(config);
            services.AddCarts();
        }
    }
}
