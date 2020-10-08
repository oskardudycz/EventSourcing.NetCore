using Payments.Payments.Commands;
using Core.Repositories;
using Core.Storage;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Payments.Payments.Events;

namespace Payments.Payments
{
    internal static class PaymentsConfig
    {
        internal static void AddPayments(this IServiceCollection services)
        {
            services.AddScoped<IRepository<Payment>, MartenRepository<Payment>>();

            AddCommandHandlers(services);
            AddEventHandlers(services);
        }

        private static void AddCommandHandlers(IServiceCollection services)
        {
            services.AddScoped<IRequestHandler<RequestPayment, Unit>, PaymentCommandHandler>();
            services.AddScoped<IRequestHandler<CompletePayment, Unit>, PaymentCommandHandler>();
            services.AddScoped<IRequestHandler<DiscardPayment, Unit>, PaymentCommandHandler>();
            services.AddScoped<IRequestHandler<TimeOutPayment, Unit>, PaymentCommandHandler>();
        }

        private static void AddEventHandlers(IServiceCollection services)
        {
             services.AddScoped<INotificationHandler<PaymentCompleted>, PaymentEventHandler>();
             services.AddScoped<INotificationHandler<PaymentDiscarded>, PaymentEventHandler>();
             services.AddScoped<INotificationHandler<PaymentTimedOut>, PaymentEventHandler>();
        }

        internal static void ConfigurePayments(this StoreOptions options)
        {
            // Snapshots
            options.Events.InlineProjections.AggregateStreamsWith<Payment>();
        }
    }
}
