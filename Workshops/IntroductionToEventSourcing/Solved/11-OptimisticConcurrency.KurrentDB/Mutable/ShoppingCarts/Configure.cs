using KurrentDB.Client;
using OptimisticConcurrency.Core.KurrentDB;
using OptimisticConcurrency.Mutable.Pricing;

namespace OptimisticConcurrency.Mutable.ShoppingCarts;

public static class Configure
{
    public static IServiceCollection AddMutableShoppingCarts(this IServiceCollection services) =>
        services.AddSingleton<IProductPriceCalculator>(FakeProductPriceCalculator.Returning(100));

    public static Task<StreamState> GetAndUpdate(
        this KurrentDBClient eventStore,
        Guid id,
        StreamState expectedRevision,
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

    public static Task<(ShoppingCart?, StreamState?)> GetShoppingCart(
        this KurrentDBClient eventStore,
        Guid id,
        CancellationToken ct
    ) =>
        eventStore.AggregateStream<ShoppingCart, ShoppingCartEvent>(
            ShoppingCart.Initial,
            id,
            ct
        );
}
