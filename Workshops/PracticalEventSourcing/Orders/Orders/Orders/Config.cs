using Orders.Orders.Commands;
using Core.Repositories;
using Core.Storage;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Orders.Carts.Events;
using Orders.Orders.Events;
using Orders.Payments.Events;
using Orders.Shipments.Events;

namespace Orders.Orders
{
    internal static class OrdersConfig
    {
        internal static void AddOrders(this IServiceCollection services)
        {
            services.AddScoped<IRepository<Order>, MartenRepository<Order>>();

            AddCommandHandlers(services);
            AddQueryHandlers(services);
            AddEventHandlers(services);
        }

        private static void AddCommandHandlers(IServiceCollection services)
        {
            services.AddScoped<IRequestHandler<InitOrder, Unit>, OrderCommandHandler>();
            services.AddScoped<IRequestHandler<RecordOrderPayment, Unit>, OrderCommandHandler>();
            services.AddScoped<IRequestHandler<CompleteOrder, Unit>, OrderCommandHandler>();
            services.AddScoped<IRequestHandler<CancelOrder, Unit>, OrderCommandHandler>();
        }

        private static void AddQueryHandlers(IServiceCollection services)
        {
            // services.AddScoped<IRequestHandler<GetOrderById, OrderDetails>, OrderQueryHandler>();
            // services.AddScoped<IRequestHandler<GetOrderAtVersion, OrderDetails>, OrderQueryHandler>();
            // services.AddScoped<IRequestHandler<GetOrders, IPagedList<OrderShortInfo>>, OrderQueryHandler>();
            // services
            //     .AddScoped<IRequestHandler<GetOrderHistory, IPagedList<OrderHistory>>, OrderQueryHandler>();
        }

        private static void AddEventHandlers(IServiceCollection services)
        {
            services.AddScoped<INotificationHandler<CartFinalized>, OrderSaga>();
            services.AddScoped<INotificationHandler<OrderInitialized>, OrderSaga>();
            services.AddScoped<INotificationHandler<PaymentFinalized>, OrderSaga>();
            services.AddScoped<INotificationHandler<PackageWasSent>, OrderSaga>();
            services.AddScoped<INotificationHandler<ProductWasOutOfStock>, OrderSaga>();
            services.AddScoped<INotificationHandler<OrderCancelled>, OrderSaga>();
            services.AddScoped<INotificationHandler<OrderPaymentRecorded>, OrderSaga>();
        }

        internal static void ConfigureOrders(this StoreOptions options)
        {
            // Snapshots
            options.Events.InlineProjections.AggregateStreamsWith<Order>();
            // options.Schema.For<Order>().Index(x => x.SeatId, x =>
            // {
            //     x.IsUnique = true;
            //
            //     // Partial index by supplying a condition
            //     x.Where = "(data ->> 'Status') != 'Cancelled'";
            // });
            // options.Schema.For<Order>().Index(x => x.Number, x =>
            // {
            //     x.IsUnique = true;
            //
            //     // Partial index by supplying a condition
            //     x.Where = "(data ->> 'Status') != 'Cancelled'";
            // });
            //
            //
            // // options.Schema.For<Order>().UniqueIndex(x => x.SeatId);
            //
            // // projections
            // options.Events.InlineProjections.Add<OrderDetailsProjection>();
            // options.Events.InlineProjections.Add<OrderShortInfoProjection>();
            //
            // // transformation
            // options.Events.InlineProjections.TransformEvents<TentativeOrderCreated, OrderHistory>(new OrderHistoryTransformation());
            // options.Events.InlineProjections.TransformEvents<OrderSeatChanged, OrderHistory>(new OrderHistoryTransformation());
            // options.Events.InlineProjections.TransformEvents<OrderConfirmed, OrderHistory>(new OrderHistoryTransformation());
            // options.Events.InlineProjections.TransformEvents<OrderCancelled, OrderHistory>(new OrderHistoryTransformation());
        }
    }
}
