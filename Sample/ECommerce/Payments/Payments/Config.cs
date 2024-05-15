using Core.Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.Payments;

namespace Payments;

public static class Config
{
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services, IConfiguration config)
    {
        return services.AddMarten(config, options =>
        {
            options.ConfigurePayments();
            options.DisableNpgsqlLogging = true;
        })
        .UseNpgsqlDataSource()
        .Services
        .AddPayments();
    }
}
