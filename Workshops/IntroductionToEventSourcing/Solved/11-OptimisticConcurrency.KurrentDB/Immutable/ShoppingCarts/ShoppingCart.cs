using OptimisticConcurrency.Immutable.Pricing;

namespace OptimisticConcurrency.Immutable.ShoppingCarts;
using static ShoppingCartEvent;

// EVENTS
public abstract record ShoppingCartEvent
{
    public record ShoppingCartOpened(
        Guid ShoppingCartId,
        Guid ClientId,
        DateTimeOffset OpenedAt
    ): ShoppingCartEvent;

    public record ProductItemAddedToShoppingCart(
        Guid ShoppingCartId,
        PricedProductItem ProductItem,
        DateTimeOffset AddedAt
    ): ShoppingCartEvent;

    public record ProductItemRemovedFromShoppingCart(
        Guid ShoppingCartId,
        PricedProductItem ProductItem,
        DateTimeOffset RemovedAt
    ): ShoppingCartEvent;

    public record ShoppingCartConfirmed(
        Guid ShoppingCartId,
        DateTimeOffset ConfirmedAt
    ): ShoppingCartEvent;

    public record ShoppingCartCanceled(
        Guid ShoppingCartId,
        DateTimeOffset CanceledAt
    ): ShoppingCartEvent;

    // This won't allow external inheritance
    private ShoppingCartEvent(){}
}

// ENTITY
public record ShoppingCart(
    Guid Id,
    Guid ClientId,
    ShoppingCartStatus Status,
    PricedProductItem[] ProductItems,
    DateTimeOffset OpenedAt,
    DateTimeOffset? ConfirmedAt = null,
    DateTimeOffset? CanceledAt = null
)
{
    public bool IsClosed => ShoppingCartStatus.Closed.HasFlag(Status);

    public bool HasEnough(PricedProductItem productItem)
    {
        var (productId, quantity, _) = productItem;
        var currentQuantity = ProductItems.Where(pi => pi.ProductId == productId)
            .Select(pi => pi.Quantity)
            .FirstOrDefault();

        return currentQuantity >= quantity;
    }

    public static ShoppingCart Initial() =>
        new (Guid.Empty, Guid.Empty, default, [], default);

    public static ShoppingCart Evolve(ShoppingCart state, ShoppingCartEvent @event) =>
        @event switch
        {
            ShoppingCartOpened(var shoppingCartId, var clientId, _) =>
                state with
                {
                    Id = shoppingCartId,
                    ClientId = clientId,
                    Status = ShoppingCartStatus.Pending
                },
            ProductItemAddedToShoppingCart(_, var pricedProductItem, _) =>
                state with
                {
                    ProductItems = state.ProductItems
                        .Concat([pricedProductItem])
                        .GroupBy(pi => pi.ProductId)
                        .Select(group => group.Count() == 1?
                            group.First()
                            : new PricedProductItem(
                                group.Key,
                                group.Sum(pi => pi.Quantity),
                                group.First().UnitPrice
                            )
                        )
                        .ToArray()
                },
            ProductItemRemovedFromShoppingCart(_, var pricedProductItem, _) =>
                state with
                {
                    ProductItems = state.ProductItems
                        .Select(pi => pi.ProductId == pricedProductItem.ProductId?
                            pi with { Quantity = pi.Quantity - pricedProductItem.Quantity }
                            :pi
                        )
                        .Where(pi => pi.Quantity > 0)
                        .ToArray()
                },
            ShoppingCartConfirmed(_, var confirmedAt) =>
                state with
                {
                    Status = ShoppingCartStatus.Confirmed,
                    ConfirmedAt = confirmedAt
                },
            ShoppingCartCanceled(_, var canceledAt) =>
                state with
                {
                    Status = ShoppingCartStatus.Canceled,
                    CanceledAt = canceledAt
                },
            _ => state
        };
}

public enum ShoppingCartStatus
{
    Pending = 1,
    Confirmed = 2,
    Canceled = 4,

    Closed = Confirmed | Canceled
}
