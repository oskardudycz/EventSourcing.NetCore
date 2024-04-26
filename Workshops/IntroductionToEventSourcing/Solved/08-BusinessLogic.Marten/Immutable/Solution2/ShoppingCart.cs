namespace IntroductionToEventSourcing.BusinessLogic.Immutable.Solution2;
using static ShoppingCartEvent;

// EVENTS
public abstract record ShoppingCartEvent
{
    public record ShoppingCartOpened(
        Guid ShoppingCartId,
        Guid ClientId
    ): ShoppingCartEvent;

    public record ProductItemAddedToShoppingCart(
        Guid ShoppingCartId,
        PricedProductItem ProductItem
    ): ShoppingCartEvent;

    public record ProductItemRemovedFromShoppingCart(
        Guid ShoppingCartId,
        PricedProductItem ProductItem
    ): ShoppingCartEvent;

    public record ShoppingCartConfirmed(
        Guid ShoppingCartId,
        DateTime ConfirmedAt
    ): ShoppingCartEvent;

    public record ShoppingCartCanceled(
        Guid ShoppingCartId,
        DateTime CanceledAt
    ): ShoppingCartEvent;

    // This won't allow
    private ShoppingCartEvent(){}
}

// ENTITY
public record ShoppingCart(
    Guid Id,
    Guid ClientId,
    ShoppingCartStatus Status,
    PricedProductItem[] ProductItems,
    DateTime? ConfirmedAt = null,
    DateTime? CanceledAt = null
){
    public bool IsClosed => ShoppingCartStatus.Closed.HasFlag(Status);

    public static ShoppingCart Create(ShoppingCartOpened opened) =>
        new ShoppingCart(
            opened.ShoppingCartId,
            opened.ClientId,
            ShoppingCartStatus.Pending,
            Array.Empty<PricedProductItem>()
        );

    public ShoppingCart Apply(ProductItemAddedToShoppingCart productItemAdded) =>
        this with
        {
            ProductItems = ProductItems
                .Concat(new[] { productItemAdded.ProductItem })
                .GroupBy(pi => pi.ProductId)
                .Select(group => group.Count() == 1
                    ? group.First()
                    : new PricedProductItem(
                        group.Key,
                        group.Sum(pi => pi.Quantity),
                        group.First().UnitPrice
                    )
                )
                .ToArray()
        };

    public ShoppingCart Apply(ProductItemRemovedFromShoppingCart productItemRemoved) =>
        this with
        {
            ProductItems = ProductItems
                .Select(pi => pi.ProductId == productItemRemoved.ProductItem.ProductId
                    ? new PricedProductItem(
                        pi.ProductId,
                        pi.Quantity - productItemRemoved.ProductItem.Quantity,
                        pi.UnitPrice
                    )
                    : pi
                )
                .Where(pi => pi.Quantity > 0)
                .ToArray()
        };

    public ShoppingCart Apply(ShoppingCartConfirmed confirmed) =>
        this with
        {
            Status = ShoppingCartStatus.Confirmed,
            ConfirmedAt = confirmed.ConfirmedAt
        };

    public ShoppingCart Apply(ShoppingCartCanceled canceled) =>
        this with
        {
            Status = ShoppingCartStatus.Canceled,
            CanceledAt = canceled.CanceledAt
        };

    public bool HasEnough(PricedProductItem productItem)
    {
        var (productId, quantity, _) = productItem;
        var currentQuantity = ProductItems.Where(pi => pi.ProductId == productId)
            .Select(pi => pi.Quantity)
            .FirstOrDefault();

        return currentQuantity >= quantity;
    }
}

public enum ShoppingCartStatus
{
    Pending = 1,
    Confirmed = 2,
    Canceled = 4,

    Closed = Confirmed | Canceled
}

// VALUE OBJECTS
public record ProductItem(
    Guid ProductId,
    int Quantity
);

public record PricedProductItem(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice
);
