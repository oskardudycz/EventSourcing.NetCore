using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Orders.Payments.DiscardingPayment;
using Orders.Payments.RequestingPayment;

namespace Orders.Payments
{
    internal static class PaymentsConfig
    {
        internal static IServiceCollection AddPayments(this IServiceCollection services)
        {
            return services.AddScoped<IRequestHandler<DiscardPayment, Unit>, HandleDiscardPayment>()
                .AddScoped<IRequestHandler<RequestPayment, Unit>, HandleRequestPayment>();
        }
    }
}
