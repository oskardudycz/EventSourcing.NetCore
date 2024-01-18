using Carts.Pricing;
using Carts.ShoppingCarts.AddingProduct;
using Carts.ShoppingCarts.CancelingCart;
using Carts.ShoppingCarts.ConfirmingCart;
using Carts.ShoppingCarts.FinalizingCart;
using Carts.ShoppingCarts.GettingCartAtVersion;
using Carts.ShoppingCarts.GettingCartById;
using Carts.ShoppingCarts.GettingCartHistory;
using Carts.ShoppingCarts.GettingCarts;
using Carts.ShoppingCarts.OpeningCart;
using Carts.ShoppingCarts.RemovingProduct;
using Core.Commands;
using Core.Events;
using Core.Marten.Repository;
using Core.Queries;
using Marten;
using Marten.Events.Projections;
using Marten.Pagination;
using Microsoft.Extensions.DependencyInjection;

namespace Carts.ShoppingCarts;

internal static class CartsConfig
{
    internal static IServiceCollection AddCarts(this IServiceCollection services) =>
        services.AddScoped<IProductPriceCalculator, RandomProductPriceCalculator>()
            .AddMartenRepository<ShoppingCart>()
            .AddCommandHandlers()
            .AddQueryHandlers()
            .AddEventHandlers();

    private static IServiceCollection AddCommandHandlers(this IServiceCollection services) =>
        services.AddCommandHandler<OpenShoppingCart, HandleOpenShoppingCart>()
            .AddCommandHandler<AddProduct, HandleAddProduct>()
            .AddCommandHandler<RemoveProduct, HandleRemoveProduct>()
            .AddCommandHandler<ConfirmShoppingCart, HandleConfirmShoppingCart>()
            .AddCommandHandler<CancelShoppingCart, HandleCancelShoppingCart>();

    private static IServiceCollection AddQueryHandlers(this IServiceCollection services) =>
        services.AddQueryHandler<GetCartById, ShoppingCartDetails?, HandleGetCartById>()
            .AddQueryHandler<GetCartAtVersion, ShoppingCartDetails, HandleGetCartAtVersion>()
            .AddQueryHandler<GetCarts, IPagedList<ShoppingCartShortInfo>, HandleGetCarts>()
            .AddQueryHandler<GetCartHistory, IPagedList<ShoppingCartHistory>, HandleGetCartHistory>();

    private static IServiceCollection AddEventHandlers(this IServiceCollection services) =>
        services.AddEventHandler<EventEnvelope<ShoppingCartConfirmed>, HandleCartFinalized>();

    internal static void ConfigureCarts(this StoreOptions options)
    {
        // Snapshots
        options.Projections.Snapshot<ShoppingCart>(SnapshotLifecycle.Inline);
        // // projections
        options.Projections.Add<CartShortInfoProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<CartDetailsProjection>(ProjectionLifecycle.Inline);
        //
        // // transformation
        options.Projections.Add<CartHistoryTransformation>(ProjectionLifecycle.Inline);
    }
}
