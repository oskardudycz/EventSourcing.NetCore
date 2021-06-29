using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Orders.Payments.Commands;

namespace Orders.Payments
{
    internal static class PaymentsConfig
    {
        internal static IServiceCollection AddPayments(this IServiceCollection services)
        {
            return services.AddScoped<IRequestHandler<DiscardPayment, Unit>, PaymentsCommandHandler>()
                .AddScoped<IRequestHandler<RequestPayment, Unit>, PaymentsCommandHandler>();
        }
    }
}
