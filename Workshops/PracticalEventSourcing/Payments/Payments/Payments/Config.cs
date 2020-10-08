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
            AddQueryHandlers(services);
            AddEventHandlers(services);
        }

        private static void AddCommandHandlers(IServiceCollection services)
        {
            services.AddScoped<IRequestHandler<RequestPayment, Unit>, PaymentCommandHandler>();
            services.AddScoped<IRequestHandler<CompletePayment, Unit>, PaymentCommandHandler>();
            services.AddScoped<IRequestHandler<DiscardPayment, Unit>, PaymentCommandHandler>();
            services.AddScoped<IRequestHandler<TimeOutPayment, Unit>, PaymentCommandHandler>();
        }

        private static void AddQueryHandlers(IServiceCollection services)
        {
            // services.AddScoped<IRequestHandler<GetPaymentById, PaymentDetails>, PaymentQueryHandler>();
            // services.AddScoped<IRequestHandler<GetPaymentAtVersion, PaymentDetails>, PaymentQueryHandler>();
            // services.AddScoped<IRequestHandler<GetPayments, IPagedList<PaymentShortInfo>>, PaymentQueryHandler>();
            // services
            //     .AddScoped<IRequestHandler<GetPaymentHistory, IPagedList<PaymentHistory>>, PaymentQueryHandler>();
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
            // options.Schema.For<Payment>().Index(x => x.SeatId, x =>
            // {
            //     x.IsUnique = true;
            //
            //     // Partial index by supplying a condition
            //     x.Where = "(data ->> 'Status') != 'Cancelled'";
            // });
            // options.Schema.For<Payment>().Index(x => x.Number, x =>
            // {
            //     x.IsUnique = true;
            //
            //     // Partial index by supplying a condition
            //     x.Where = "(data ->> 'Status') != 'Cancelled'";
            // });
            //
            //
            // // options.Schema.For<Payment>().UniqueIndex(x => x.SeatId);
            //
            // // projections
            // options.Events.InlineProjections.Add<PaymentDetailsProjection>();
            // options.Events.InlineProjections.Add<PaymentShortInfoProjection>();
            //
            // // transformation
            // options.Events.InlineProjections.TransformEvents<TentativePaymentCreated, PaymentHistory>(new PaymentHistoryTransformation());
            // options.Events.InlineProjections.TransformEvents<PaymentSeatChanged, PaymentHistory>(new PaymentHistoryTransformation());
            // options.Events.InlineProjections.TransformEvents<PaymentConfirmed, PaymentHistory>(new PaymentHistoryTransformation());
            // options.Events.InlineProjections.TransformEvents<PaymentCancelled, PaymentHistory>(new PaymentHistoryTransformation());
        }
    }
}
