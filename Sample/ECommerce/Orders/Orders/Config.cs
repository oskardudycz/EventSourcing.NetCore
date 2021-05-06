using Orders.Orders;
using Core.Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Payments;
using Orders.Shipments;

namespace Orders
{
    public static class Config
    {
        public static IServiceCollection AddOrdersModule(this IServiceCollection services, IConfiguration config)
        {
            return services
                .AddSingleton(sp =>
                    config.GetSection(ExternalServicesConfig.ConfigName).Get<ExternalServicesConfig>()
                )
                .AddMarten(config, options =>
                {
                    options.ConfigureOrders();
                })
                .AddOrders()
                .AddPayments()
                .AddShipments();
        }
    }

    public class ExternalServicesConfig
    {
        public static string ConfigName = "ExternalServices";

        public string PaymentsUrl { get; set; }
        public string ShipmentsUrl { get; set; }
    }
}
