using EventStore.Client;
using OptimisticConcurrency.Core.EventStoreDB;
using OptimisticConcurrency.Mixed.Pricing;

namespace OptimisticConcurrency.Mixed.ShoppingCarts;

public static class Configure
{
    public  static IServiceCollection AddMixedShoppingCarts(this IServiceCollection services) =>
        services.AddSingleton<IProductPriceCalculator>(FakeProductPriceCalculator.Returning(100));

    public static Task<StreamRevision> GetAndUpdate(
        this EventStoreClient eventStore,
        Guid id,
        StreamRevision expectedRevision,
        Func<ShoppingCart, ShoppingCartEvent[]> handle,
        CancellationToken ct
    ) =>
        eventStore.GetAndUpdate(
            (state, @event) =>
            {
                state.Evolve(@event);
                return state;
            },
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
            (state, @event) =>
            {
                state.Evolve(@event);
                return state;
            },
            ShoppingCart.Initial,
            id,
            ct
        );
}
