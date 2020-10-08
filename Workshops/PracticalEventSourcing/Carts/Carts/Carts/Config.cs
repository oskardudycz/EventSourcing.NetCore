using Carts.Carts.Commands;
using Carts.Carts.Events;
using Carts.Carts.Projections;
using Carts.Carts.Queries;
using Carts.Pricing;
using Core.Repositories;
using Core.Storage;
using Marten;
using Marten.Pagination;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Carts.Carts
{
    internal static class CartsConfig
    {
        internal static void AddCarts(this IServiceCollection services)
        {
            services.AddScoped<IProductPriceCalculator, RandomProductPriceCalculator>();

            services.AddScoped<IRepository<Cart>, MartenRepository<Cart>>();

            AddCommandHandlers(services);
            AddQueryHandlers(services);
            AddEventHandlers(services);
        }

        private static void AddCommandHandlers(IServiceCollection services)
        {
            services.AddScoped<IRequestHandler<InitCart, Unit>, CartCommandHandler>();
            services.AddScoped<IRequestHandler<AddProduct, Unit>, CartCommandHandler>();
            services.AddScoped<IRequestHandler<RemoveProduct, Unit>, CartCommandHandler>();
            services.AddScoped<IRequestHandler<ConfirmCart, Unit>, CartCommandHandler>();
        }

        private static void AddQueryHandlers(IServiceCollection services)
        {
            services.AddScoped<IRequestHandler<GetCartById, CartDetails>, CartQueryHandler>();
            services.AddScoped<IRequestHandler<GetCartAtVersion, CartDetails>, CartQueryHandler>();
            services.AddScoped<IRequestHandler<GetCarts, IPagedList<CartShortInfo>>, CartQueryHandler>();
            services
                 .AddScoped<IRequestHandler<GetCartHistory, IPagedList<CartHistory>>, CartQueryHandler>();
        }

        private static void AddEventHandlers(IServiceCollection services)
        {
             services.AddScoped<INotificationHandler<CartConfirmed>, CartEventHandler>();
        }

        internal static void ConfigureCarts(this StoreOptions options)
        {
            // Snapshots
            options.Events.InlineProjections.AggregateStreamsWith<Cart>();
            // // projections
            options.Events.InlineProjections.Add<CartShortInfoProjection>();
            options.Events.InlineProjections.Add<CartDetailsProjection>();
            //
            // // transformation
            options.Events.InlineProjections.TransformEvents<CartInitialized, CartHistory>(new CartHistoryTransformation());
            options.Events.InlineProjections.TransformEvents<ProductAdded, CartHistory>(new CartHistoryTransformation());
            options.Events.InlineProjections.TransformEvents<CartConfirmed, CartHistory>(new CartHistoryTransformation());
            options.Events.InlineProjections.TransformEvents<ProductRemoved, CartHistory>(new CartHistoryTransformation());
        }
    }
}
