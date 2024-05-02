using ApplicationLogic.EventStoreDB.Mutable.Pricing;
using EventStore.Client;
using ApplicationLogic.EventStoreDB.Core.EventStoreDB;

namespace ApplicationLogic.EventStoreDB.Mutable.ShoppingCarts;

public static class Configure
{
    public static IServiceCollection AddMutableShoppingCarts(this IServiceCollection services) =>
        services.AddSingleton<IProductPriceCalculator>(FakeProductPriceCalculator.Returning(100));

    public static Task GetAndUpdate(
        this EventStoreClient eventStore,
        Guid id,
        Action<ShoppingCart> handle,
        CancellationToken ct
    ) =>
        eventStore.GetAndUpdate<ShoppingCart, ShoppingCartEvent>(
            ShoppingCart.Initial,
            id,
            handle,
            ct
        );

    public static Task<ShoppingCart?> GetShoppingCart(
        this EventStoreClient eventStore,
        Guid id,
        CancellationToken ct
    ) =>
        eventStore.AggregateStream<ShoppingCart, ShoppingCartEvent>(
            ShoppingCart.Initial,
            id,
            ct
        );
}
