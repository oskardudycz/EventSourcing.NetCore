namespace IntroductionToEventSourcing.BusinessLogic.Slimmed.Immutable;
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

// VALUE OBJECTS
public record PricedProductItem(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice
)
{
    public decimal TotalPrice => Quantity * UnitPrice;
}

public record ProductItem(Guid ProductId, int Quantity);


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

    public static ShoppingCart Default() =>
        new (default, default, default, [], default);

    public static ShoppingCart Evolve(ShoppingCart shoppingCart, ShoppingCartEvent @event)
    {
        return @event switch
        {
            ShoppingCartOpened(var shoppingCartId, var clientId, var openedAt) =>
                shoppingCart with
                {
                    Id = shoppingCartId,
                    ClientId = clientId,
                    Status = ShoppingCartStatus.Pending,
                    ProductItems = [],
                    OpenedAt = openedAt
                },
            ProductItemAddedToShoppingCart(_, var pricedProductItem, _) =>
                shoppingCart with
                {
                    ProductItems = shoppingCart.ProductItems
                        .Concat(new [] { pricedProductItem })
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
                shoppingCart with
                {
                    ProductItems = shoppingCart.ProductItems
                        .Select(pi => pi.ProductId == pricedProductItem.ProductId?
                            pi with { Quantity = pi.Quantity - pricedProductItem.Quantity }
                            :pi
                        )
                        .Where(pi => pi.Quantity > 0)
                        .ToArray()
                },
            ShoppingCartConfirmed(_, var confirmedAt) =>
                shoppingCart with
                {
                    Status = ShoppingCartStatus.Confirmed,
                    ConfirmedAt = confirmedAt
                },
            ShoppingCartCanceled(_, var canceledAt) =>
                shoppingCart with
                {
                    Status = ShoppingCartStatus.Canceled,
                    CanceledAt = canceledAt
                },
            _ => shoppingCart
        };
    }
}

public enum ShoppingCartStatus
{
    Pending = 1,
    Confirmed = 2,
    Canceled = 4,

    Closed = Confirmed | Canceled
}
