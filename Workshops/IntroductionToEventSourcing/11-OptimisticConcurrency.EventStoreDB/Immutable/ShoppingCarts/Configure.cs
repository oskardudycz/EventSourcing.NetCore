using EventStore.Client;
using OptimisticConcurrency.Core.EventStoreDB;
using OptimisticConcurrency.Immutable.Pricing;

namespace OptimisticConcurrency.Immutable.ShoppingCarts;

public static class Configure
{
    public  static IServiceCollection AddImmutableShoppingCarts(this IServiceCollection services) =>
        services.AddSingleton<IProductPriceCalculator>(FakeProductPriceCalculator.Returning(100));
    public static Task GetAndUpdate(
        this EventStoreClient eventStore,
        Guid id,
        Func<ShoppingCart, ShoppingCartEvent[]> handle,
        CancellationToken ct
    ) =>
        eventStore.GetAndUpdate(
            ShoppingCart.Evolve,
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
            ShoppingCart.Evolve,
            ShoppingCart.Initial,
            id,
            ct
        );
}
