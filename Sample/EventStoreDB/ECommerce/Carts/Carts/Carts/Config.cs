using Carts.Carts.AddingProduct;
using Carts.Carts.ConfirmingCart;
using Carts.Carts.GettingCartAtVersion;
using Carts.Carts.GettingCartById;
using Carts.Carts.GettingCartHistory;
using Carts.Carts.GettingCarts;
using Carts.Carts.InitializingCart;
using Carts.Carts.Queries;
using Carts.Carts.RemovingProduct;
using Carts.Pricing;
using Core.EventStoreDB.Repository;
using Core.Marten.ExternalProjections;
using Core.Repositories;
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

            services.AddScoped<IRepository<Cart>, EventStoreDBRepository<Cart>>();

            AddCommandHandlers(services);
            AddProjections(services);
            AddQueryHandlers(services);
        }

        private static void AddCommandHandlers(IServiceCollection services)
        {
            services.AddScoped<IRequestHandler<InitializeCart, Unit>, HandleInitializeCart>();
            services.AddScoped<IRequestHandler<AddProduct, Unit>, HandleAddProduct>();
            services.AddScoped<IRequestHandler<RemoveProduct, Unit>, HandleRemoveProduct>();
            services.AddScoped<IRequestHandler<ConfirmCart, Unit>, HandleConfirmCart>();
        }

        private static void AddProjections(IServiceCollection services)
        {
            services
                .Project<CartInitialized, CartDetails>(@event => @event.CartId)
                .Project<ProductAdded, CartDetails>(@event => @event.CartId)
                .Project<ProductRemoved, CartDetails>(@event => @event.CartId)
                .Project<CartConfirmed, CartDetails>(@event => @event.CartId);

            services
                .Project<CartInitialized, CartShortInfo>(@event => @event.CartId)
                .Project<ProductAdded, CartShortInfo>(@event => @event.CartId)
                .Project<ProductRemoved, CartShortInfo>(@event => @event.CartId)
                .Project<CartConfirmed, CartShortInfo>(@event => @event.CartId);

            services
                .Project<CartInitialized, CartHistory>(@event => @event.CartId)
                .Project<ProductAdded, CartHistory>(@event => @event.CartId)
                .Project<ProductRemoved, CartHistory>(@event => @event.CartId)
                .Project<CartConfirmed, CartHistory>(@event => @event.CartId);
        }

        private static void AddQueryHandlers(IServiceCollection services)
        {
            services.AddScoped<IRequestHandler<GetCartById, CartDetails>, HandleGetCartById>();
            services.AddScoped<IRequestHandler<GetCarts, IPagedList<CartShortInfo>>, HandleGetCarts>();
            services.AddScoped<IRequestHandler<GetCartHistory, IPagedList<CartHistory>>, HandleGetCartHistory>();
            services.AddScoped<IRequestHandler<GetCartAtVersion, CartDetails>, HandleGetCartAtVersion>();
        }
    }
}
