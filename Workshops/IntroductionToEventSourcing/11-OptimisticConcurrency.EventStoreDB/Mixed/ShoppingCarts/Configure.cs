using EventStore.Client;
using OptimisticConcurrency.Core.EventStoreDB;
using OptimisticConcurrency.Mixed.Pricing;

namespace OptimisticConcurrency.Mixed.ShoppingCarts;

public static class Configure
{
    public  static IServiceCollection AddMixedShoppingCarts(this IServiceCollection services) =>
        services.AddSingleton<IProductPriceCalculator>(FakeProductPriceCalculator.Returning(100));

    public static Task GetAndUpdate(
        this EventStoreClient eventStore,
        Guid id,
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
            handle,
            ct
        );

    public static Task<ShoppingCart?> GetShoppingCart(
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
