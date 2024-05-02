using ApplicationLogic.EventStoreDB.Mixed.Pricing;
using EventStore.Client;
using ApplicationLogic.EventStoreDB.Core.EventStoreDB;

namespace ApplicationLogic.EventStoreDB.Mixed.ShoppingCarts;

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
