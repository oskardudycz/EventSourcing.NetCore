using ApplicationLogic.KurrentDB.Mutable.Pricing;
using KurrentDB.Client;
using ApplicationLogic.KurrentDB.Core.KurrentDB;

namespace ApplicationLogic.KurrentDB.Mutable.ShoppingCarts;

public static class Configure
{
    public static IServiceCollection AddMutableShoppingCarts(this IServiceCollection services) =>
        services.AddSingleton<IProductPriceCalculator>(FakeProductPriceCalculator.Returning(100));

    public static Task GetAndUpdate(
        this KurrentDBClient eventStore,
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
