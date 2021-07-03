using Core.Marten.Repository;
using Core.Repositories;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Payments.Payments.CompletingPayment;
using Payments.Payments.DiscardingPayment;
using Payments.Payments.FailingPayment;
using Payments.Payments.FinalizingPayment;
using Payments.Payments.RequestingPayment;
using Payments.Payments.TimingOutPayment;

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
            services.AddScoped<IRequestHandler<RequestPayment, Unit>, HandleRequestPayment>();
            services.AddScoped<IRequestHandler<CompletePayment, Unit>, HandleCompletePayment>();
            services.AddScoped<IRequestHandler<DiscardPayment, Unit>, HandleDiscardPayment>();
            services.AddScoped<IRequestHandler<TimeOutPayment, Unit>, HandleTimeOutPayment>();
        }

        private static void AddEventHandlers(IServiceCollection services)
        {
             services.AddScoped<INotificationHandler<PaymentCompleted>, TransformIntoPaymentFinalized>();
             services.AddScoped<INotificationHandler<PaymentDiscarded>, TransformIntoPaymentFailed>();
             services.AddScoped<INotificationHandler<PaymentTimedOut>, TransformIntoPaymentFailed>();
        }

        internal static void ConfigurePayments(this StoreOptions options)
        {
            // Snapshots
            options.Projections.SelfAggregate<Payment>();
        }
    }
}
