using Carts.Carts.AddingProduct;
using Carts.Carts.ConfirmingCart;
using Carts.Carts.GettingCartAtVersion;
using Carts.Carts.GettingCartById;
using Carts.Carts.GettingCartHistory;
using Carts.Carts.GettingCarts;
using Carts.Carts.InitializingCart;
using Carts.Carts.RemovingProduct;
using Carts.Pricing;
using Core.Commands;
using Core.EventStoreDB.Repository;
using Core.Marten.ExternalProjections;
using Core.Queries;
using Core.Repositories;
using Marten.Pagination;
using Microsoft.Extensions.DependencyInjection;

namespace Carts.Carts;

internal static class CartsConfig
{
    internal static void AddCarts(this IServiceCollection services)
    {
        services.AddScoped<IProductPriceCalculator, RandomProductPriceCalculator>()
            .AddScoped<IRepository<Cart>, EventStoreDBRepository<Cart>>()
            .AddCommandHandlers()
            .AddProjections()
            .AddQueryHandlers();
    }

    private static IServiceCollection AddCommandHandlers(this IServiceCollection services)
    {
        return services
            .AddCommandHandler<InitializeCart, HandleInitializeCart>()
            .AddCommandHandler<AddProduct, HandleAddProduct>()
            .AddCommandHandler<RemoveProduct, HandleRemoveProduct>()
            .AddCommandHandler<ConfirmCart, HandleConfirmCart>();
    }

    private static IServiceCollection AddProjections(this IServiceCollection services)
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

        return services;
    }

    private static IServiceCollection AddQueryHandlers(this IServiceCollection services)
    {
        return services
            .AddQueryHandler<GetCartById, CartDetails, HandleGetCartById>()
            .AddQueryHandler<GetCarts, IPagedList<CartShortInfo>, HandleGetCarts>()
            .AddQueryHandler<GetCartHistory, IPagedList<CartHistory>, HandleGetCartHistory>()
            .AddQueryHandler<GetCartAtVersion, CartDetails, HandleGetCartAtVersion>();
    }
}