using ApplicationLogic.EventStoreDB.Core.Marten;
using ApplicationLogic.EventStoreDB.Mixed.Pricing;
using Marten;

namespace ApplicationLogic.EventStoreDB.Mixed.ShoppingCarts;
using static ShoppingCartEvent;

public static class Configure
{
    private const string ModulePrefix = "Mixed";

    public  static IServiceCollection AddMixedShoppingCarts(this IServiceCollection services)
    {
        services.AddSingleton<IProductPriceCalculator>(FakeProductPriceCalculator.Returning(100));

        return services;
    }
    public static StoreOptions ConfigureMixedShoppingCarts(this StoreOptions options)
    {
        options.Projections.LiveStreamAggregation<MixedShoppingCart>();

        // this is needed as we're sharing document store and have event types with the same name
        options.MapEventWithPrefix<ShoppingCartOpened>(ModulePrefix);
        options.MapEventWithPrefix<ProductItemAddedToShoppingCart>(ModulePrefix);
        options.MapEventWithPrefix<ProductItemRemovedFromShoppingCart>(ModulePrefix);
        options.MapEventWithPrefix<ShoppingCartConfirmed>(ModulePrefix);
        options.MapEventWithPrefix<ShoppingCartCanceled>(ModulePrefix);

        return options;
    }
}
