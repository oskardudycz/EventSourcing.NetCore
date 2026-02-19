using Core.Configuration;
using Orders.Orders;
using Core.Marten;
using JasperFx.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Payments;
using Orders.Shipments;

namespace Orders;

public static class Config
{
    public static IServiceCollection AddOrdersModule(this IServiceCollection services, IConfiguration config) =>
        services
            .AddSingleton(sp =>
                config.GetRequiredConfig<ExternalServicesConfig>(ExternalServicesConfig.ConfigName)
            )
            .AddMarten(config, options =>
            {
                options.Projections.DaemonLockId = 44444;
                options.ConfigureOrders();
                options.DisableNpgsqlLogging = true;
                options.Events.StreamIdentity = StreamIdentity.AsGuid;
            })
            .UseNpgsqlDataSource()
            .Services
            .AddOrders()
            .AddPayments()
            .AddShipments();
}

public class ExternalServicesConfig
{
    public static string ConfigName = "ExternalServices";

    public string? PaymentsUrl { get; set; }
    public string? ShipmentsUrl { get; set; }
}
