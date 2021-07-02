using Carts.Carts.Commands;
using Carts.Carts.Events;
using Carts.Carts.Projections;
using Carts.Carts.Queries;
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
            services.AddScoped<IRequestHandler<InitCart, Unit>, CartCommandHandler>();
            services.AddScoped<IRequestHandler<AddProduct, Unit>, CartCommandHandler>();
            services.AddScoped<IRequestHandler<RemoveProduct, Unit>, CartCommandHandler>();
            services.AddScoped<IRequestHandler<ConfirmCart, Unit>, CartCommandHandler>();
        }

        private static void AddProjections(IServiceCollection services)
        {
            services
                .Project<CartInitialized, CartDetails>(@event => @event.CartId, (@event, cart) => cart.Apply(@event))
                .Project<ProductAdded, CartDetails>(@event => @event.CartId, (@event, cart) => cart.Apply(@event))
                .Project<ProductRemoved, CartDetails>(@event => @event.CartId, (@event, cart) => cart.Apply(@event))
                .Project<CartConfirmed, CartDetails>(@event => @event.CartId, (@event, cart) => cart.Apply(@event));

            services
                .Project<CartInitialized, CartShortInfo>(@event => @event.CartId, (@event, cart) => cart.Apply(@event))
                .Project<ProductAdded, CartShortInfo>(@event => @event.CartId, (@event, cart) => cart.Apply(@event))
                .Project<ProductRemoved, CartShortInfo>(@event => @event.CartId, (@event, cart) => cart.Apply(@event))
                .Project<CartConfirmed, CartShortInfo>(@event => @event.CartId, (@event, cart) => cart.Apply(@event));

            services
                .Project<CartInitialized, CartHistory>(@event => @event.CartId, (@event, cart) => cart.Apply(@event))
                .Project<ProductAdded, CartHistory>(@event => @event.CartId, (@event, cart) => cart.Apply(@event))
                .Project<ProductRemoved, CartHistory>(@event => @event.CartId, (@event, cart) => cart.Apply(@event))
                .Project<CartConfirmed, CartHistory>(@event => @event.CartId, (@event, cart) => cart.Apply(@event));
        }

        private static void AddQueryHandlers(IServiceCollection services)
        {
            services.AddScoped<IRequestHandler<GetCartById, CartDetails?>, CartQueryHandler>();
            services.AddScoped<IRequestHandler<GetCarts, IPagedList<CartShortInfo>>, CartQueryHandler>();
            services.AddScoped<IRequestHandler<GetCartHistory, IPagedList<CartHistory>>, CartQueryHandler>();
            services.AddScoped<IRequestHandler<GetCartAtVersion, CartDetails>, CartQueryHandler>();
        }
    }
}
