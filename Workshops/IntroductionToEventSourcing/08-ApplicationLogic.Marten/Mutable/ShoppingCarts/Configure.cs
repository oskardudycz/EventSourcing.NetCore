using ApplicationLogic.Marten.Core.Marten;
using ApplicationLogic.Marten.Mutable.Pricing;
using Marten;

namespace ApplicationLogic.Marten.Mutable.ShoppingCarts;
using static ShoppingCartEvent;

public static class Configure
{
    private const string ModulePrefix = "mutable";

    public  static IServiceCollection AddMutableShoppingCarts(this IServiceCollection services)
    {
        services.AddSingleton<IProductPriceCalculator>(FakeProductPriceCalculator.Returning(100));

        return services;
    }
    public static StoreOptions ConfigureMutableShoppingCarts(this StoreOptions options)
    {
        options.Projections.LiveStreamAggregation<MutableShoppingCart>();

        // this is needed as we're sharing document store and have event types with the same name
        options.MapEventWithPrefix<ShoppingCartOpened>(ModulePrefix);
        options.MapEventWithPrefix<ProductItemAddedToShoppingCart>(ModulePrefix);
        options.MapEventWithPrefix<ProductItemRemovedFromShoppingCart>(ModulePrefix);
        options.MapEventWithPrefix<ShoppingCartConfirmed>(ModulePrefix);
        options.MapEventWithPrefix<ShoppingCartCanceled>(ModulePrefix);

        return options;
    }
}
