using Orders.Orders;
using Core.Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Orders
{
    public static class Config
    {
        public static void AddOrdersModule(this IServiceCollection services, IConfiguration config)
        {
            services.AddMarten(config, options =>
            {
                options.ConfigureOrders();
            });
            services.AddOrders();
        }
    }
}
