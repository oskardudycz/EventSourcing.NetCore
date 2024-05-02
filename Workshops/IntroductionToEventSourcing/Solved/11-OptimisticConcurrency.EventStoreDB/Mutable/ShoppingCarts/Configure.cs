using EventStore.Client;
using OptimisticConcurrency.Core.EventStoreDB;
using OptimisticConcurrency.Mutable.Pricing;

namespace OptimisticConcurrency.Mutable.ShoppingCarts;

public static class Configure
{
    public static IServiceCollection AddMutableShoppingCarts(this IServiceCollection services) =>
        services.AddSingleton<IProductPriceCalculator>(FakeProductPriceCalculator.Returning(100));

    public static Task<StreamRevision> GetAndUpdate(
        this EventStoreClient eventStore,
        Guid id,
        StreamRevision expectedRevision,
        Action<ShoppingCart> handle,
        CancellationToken ct
    ) =>
        eventStore.GetAndUpdate<ShoppingCart, ShoppingCartEvent>(
            ShoppingCart.Initial,
            id,
            expectedRevision,
            handle,
            ct
        );

    public static Task<(ShoppingCart?, StreamRevision?)> GetShoppingCart(
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
