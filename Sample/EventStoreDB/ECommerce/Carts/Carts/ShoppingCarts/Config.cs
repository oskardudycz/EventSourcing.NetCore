using Carts.Pricing;
using Carts.ShoppingCarts.AddingProduct;
using Carts.ShoppingCarts.ConfirmingCart;
using Carts.ShoppingCarts.GettingCartAtVersion;
using Carts.ShoppingCarts.GettingCartById;
using Carts.ShoppingCarts.GettingCartHistory;
using Carts.ShoppingCarts.GettingCarts;
using Carts.ShoppingCarts.InitializingCart;
using Carts.ShoppingCarts.RemovingProduct;
using Core.Commands;
using Core.EventStoreDB.Repository;
using Core.Marten.ExternalProjections;
using Core.Queries;
using Marten.Pagination;
using Microsoft.Extensions.DependencyInjection;

namespace Carts.ShoppingCarts;

internal static class CartsConfig
{
    internal static IServiceCollection AddCarts(this IServiceCollection services)
    {
        return services.AddScoped<IProductPriceCalculator, RandomProductPriceCalculator>()
            .AddScoped<IEventStoreDBRepository<ShoppingCart>, EventStoreDBRepository<ShoppingCart>>()
            .AddCommandHandlers()
            .AddProjections()
            .AddQueryHandlers();
    }

    private static IServiceCollection AddCommandHandlers(this IServiceCollection services)
    {
        return services
            .AddCommandHandler<InitializeShoppingCart, HandleInitializeCart>()
            .AddCommandHandler<AddProduct, HandleAddProduct>()
            .AddCommandHandler<RemoveProduct, HandleRemoveProduct>()
            .AddCommandHandler<ConfirmShoppingCart, HandleConfirmCart>();
    }

    private static IServiceCollection AddProjections(this IServiceCollection services)
    {
        services
            .Project<ShoppingCartInitialized, ShoppingCartDetails>(@event => @event.CartId)
            .Project<ProductAdded, ShoppingCartDetails>(@event => @event.CartId)
            .Project<ProductRemoved, ShoppingCartDetails>(@event => @event.CartId)
            .Project<ShoppingCartConfirmed, ShoppingCartDetails>(@event => @event.CartId);

        services
            .Project<ShoppingCartInitialized, ShoppingCartShortInfo>(@event => @event.CartId)
            .Project<ProductAdded, ShoppingCartShortInfo>(@event => @event.CartId)
            .Project<ProductRemoved, ShoppingCartShortInfo>(@event => @event.CartId)
            .Project<ShoppingCartConfirmed, ShoppingCartShortInfo>(@event => @event.CartId);

        services
            .Project<ShoppingCartInitialized, CartHistory>(@event => @event.CartId)
            .Project<ProductAdded, CartHistory>(@event => @event.CartId)
            .Project<ProductRemoved, CartHistory>(@event => @event.CartId)
            .Project<ShoppingCartConfirmed, CartHistory>(@event => @event.CartId);

        return services;
    }

    private static IServiceCollection AddQueryHandlers(this IServiceCollection services)
    {
        return services
            .AddQueryHandler<GetCartById, ShoppingCartDetails, HandleGetCartById>()
            .AddQueryHandler<GetCarts, IPagedList<ShoppingCartShortInfo>, HandleGetCarts>()
            .AddQueryHandler<GetCartHistory, IPagedList<CartHistory>, HandleGetCartHistory>()
            .AddQueryHandler<GetCartAtVersion, ShoppingCartDetails, HandleGetCartAtVersion>();
    }
}
