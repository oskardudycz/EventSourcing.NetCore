using Core.Commands;
using Core.Events;
using Core.Marten.Repository;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Payments.Payments.CompletingPayment;
using Payments.Payments.DiscardingPayment;
using Payments.Payments.FailingPayment;
using Payments.Payments.FinalizingPayment;
using Payments.Payments.RequestingPayment;
using Payments.Payments.TimingOutPayment;

namespace Payments.Payments;

internal static class PaymentsConfig
{
    internal static IServiceCollection AddPayments(this IServiceCollection services) =>
        services.AddScoped<IMartenRepository<Payment>, MartenRepository<Payment>>()
            .AddCommandHandlers()
            .AddEventHandlers();

    private static IServiceCollection AddCommandHandlers(this IServiceCollection services) =>
        services
            .AddCommandHandler<RequestPayment, HandleRequestPayment>()
            .AddCommandHandler<CompletePayment, HandleCompletePayment>()
            .AddCommandHandler<DiscardPayment, HandleDiscardPayment>()
            .AddCommandHandler<TimeOutPayment, HandleTimeOutPayment>();

    private static IServiceCollection AddEventHandlers(this IServiceCollection services) =>
        services
            .AddEventHandler<PaymentCompleted, TransformIntoPaymentFinalized>()
            .AddEventHandler<PaymentDiscarded, TransformIntoPaymentFailed>()
            .AddEventHandler<PaymentTimedOut, TransformIntoPaymentFailed>();

    internal static void ConfigurePayments(this StoreOptions options)
    {
        // Snapshots
        options.Projections.SelfAggregate<Payment>();
    }
}
