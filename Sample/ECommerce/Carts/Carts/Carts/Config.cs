using Carts.Carts.AddingProduct;
using Carts.Carts.ConfirmingCart;
using Carts.Carts.FinalizingCart;
using Carts.Carts.GettingCartAtVersion;
using Carts.Carts.GettingCartById;
using Carts.Carts.GettingCartHistory;
using Carts.Carts.GettingCarts;
using Carts.Carts.InitializingCart;
using Carts.Carts.Queries;
using Carts.Carts.RemovingProduct;
using Carts.Pricing;
using Core.Marten.Repository;
using Core.Repositories;
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
            services.AddScoped<IRequestHandler<InitializeCart, Unit>, HandleInitializeCart>();
            services.AddScoped<IRequestHandler<AddProduct, Unit>, HandleAddProduct>();
            services.AddScoped<IRequestHandler<RemoveProduct, Unit>, HandleRemoveProduct>();
            services.AddScoped<IRequestHandler<ConfirmCart, Unit>, HandleConfirmCart>();
        }

        private static void AddQueryHandlers(IServiceCollection services)
        {
            services.AddScoped<IRequestHandler<GetCartById, CartDetails?>, HandleGetCartById>();
            services.AddScoped<IRequestHandler<GetCartAtVersion, CartDetails>, HandleGetCartAtVersion>();
            services.AddScoped<IRequestHandler<GetCarts, IPagedList<CartShortInfo>>, HandleGetCarts>();
            services.AddScoped<IRequestHandler<GetCartHistory, IPagedList<CartHistory>>, HandleGetCartHistory>();
        }

        private static void AddEventHandlers(IServiceCollection services)
        {
            services.AddScoped<INotificationHandler<CartConfirmed>, HandleCartFinalized>();
        }

        internal static void ConfigureCarts(this StoreOptions options)
        {
            // Snapshots
            options.Projections.SelfAggregate<Cart>();
            // // projections
            options.Projections.Add<CartShortInfoProjection>();
            options.Projections.Add<CartDetailsProjection>();
            //
            // // transformation
            options.Projections.Add<CartHistoryTransformation>();
        }
    }
}
