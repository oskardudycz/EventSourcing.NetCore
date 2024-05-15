using Core.Commands;
using Microsoft.Extensions.DependencyInjection;
using Orders.Payments.DiscardingPayment;
using Orders.Payments.RequestingPayment;

namespace Orders.Payments;

internal static class PaymentsConfig
{
    internal static IServiceCollection AddPayments(this IServiceCollection services)
    {
        services.AddHttpClient<PaymentsApiClient>(client =>
        {
            // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
            // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
            client.BaseAddress = new Uri("https+http://paymentsapi");
        });

        return services.AddCommandHandler<DiscardPayment, HandleDiscardPayment>()
            .AddCommandHandler<RequestPayment, HandleRequestPayment>();
    }
}
