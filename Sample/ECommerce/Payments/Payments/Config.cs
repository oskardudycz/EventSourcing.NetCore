using Core.Marten;
using JasperFx.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.Payments;

namespace Payments;

public static class Config
{
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services, IConfiguration config) =>
        services.AddMarten(config, options =>
            {
                options.ConfigurePayments();
                options.DisableNpgsqlLogging = true;
                options.Events.StreamIdentity = StreamIdentity.AsGuid;
            })
            .UseNpgsqlDataSource()
            .Services
            .AddPayments();
}
