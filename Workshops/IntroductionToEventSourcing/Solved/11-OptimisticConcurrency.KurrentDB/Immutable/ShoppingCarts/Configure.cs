using KurrentDB.Client;
using OptimisticConcurrency.Core.KurrentDB;
using OptimisticConcurrency.Immutable.Pricing;

namespace OptimisticConcurrency.Immutable.ShoppingCarts;

public static class Configure
{
    public  static IServiceCollection AddImmutableShoppingCarts(this IServiceCollection services) =>
        services.AddSingleton<IProductPriceCalculator>(FakeProductPriceCalculator.Returning(100));

    public static Task<StreamState> GetAndUpdate(
        this KurrentDBClient eventStore,
        Guid id,
        StreamState expectedRevision,
        Func<ShoppingCart, ShoppingCartEvent[]> handle,
        CancellationToken ct
    ) =>
        eventStore.GetAndUpdate(
            ShoppingCart.Evolve,
            ShoppingCart.Initial,
            id,
            expectedRevision,
            handle,
            ct
        );

    public static Task<(ShoppingCart?, StreamState?)> GetShoppingCart(
        this KurrentDBClient eventStore,
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
