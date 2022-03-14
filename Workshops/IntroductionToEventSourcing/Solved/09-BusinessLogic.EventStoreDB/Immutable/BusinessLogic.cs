namespace IntroductionToEventSourcing.BusinessLogic.Immutable;

// ENTITY
public record ShoppingCart(
    Guid Id,
    Guid ClientId,
    ShoppingCartStatus Status,
    PricedProductItem[] ProductItems,
    DateTime? ConfirmedAt = null,
    DateTime? CanceledAt = null
)
{
    public bool IsClosed => ShoppingCartStatus.Closed.HasFlag(Status);
    public static ShoppingCart Default() =>
        new (default, default, default, Array.Empty<PricedProductItem>());

    public static ShoppingCart When(ShoppingCart shoppingCart, object @event)
    {
        return @event switch
        {
            ShoppingCartOpened(var shoppingCartId, var clientId) =>
                shoppingCart with
                {
                    Id = shoppingCartId,
                    ClientId = clientId,
                    Status = ShoppingCartStatus.Pending
                },
            ProductItemAddedToShoppingCart(_, var pricedProductItem) =>
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
            ProductItemRemovedFromShoppingCart(_, var pricedProductItem) =>
                shoppingCart with
                {
                    ProductItems = shoppingCart.ProductItems
                        .Select(pi => pi.ProductId == pricedProductItem.ProductId?
                            new PricedProductItem(
                                pi.ProductId,
                                pi.Quantity - pricedProductItem.Quantity,
                                pi.UnitPrice
                            )
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

public record OpenShoppingCart(
    Guid ShoppingCartId,
    Guid ClientId
)
{
    public static OpenShoppingCart From(Guid? cartId, Guid? clientId)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (clientId == null || clientId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(clientId));

        return new OpenShoppingCart(cartId.Value, clientId.Value);
    }

    public static ShoppingCartOpened Handle(OpenShoppingCart command)
    {
        var (shoppingCartId, clientId) = command;

        return new ShoppingCartOpened(
            shoppingCartId,
            clientId
        );
    }
}

public record AddProductItemToShoppingCart(
    Guid ShoppingCartId,
    ProductItem ProductItem
)
{
    public static AddProductItemToShoppingCart From(Guid? cartId, ProductItem? productItem)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (productItem == null)
            throw new ArgumentOutOfRangeException(nameof(productItem));

        return new AddProductItemToShoppingCart(cartId.Value, productItem);
    }

    public static ProductItemAddedToShoppingCart Handle(
        IProductPriceCalculator productPriceCalculator,
        AddProductItemToShoppingCart command,
        ShoppingCart shoppingCart
    )
    {
        var (cartId, productItem) = command;

        if (shoppingCart.IsClosed)
            throw new InvalidOperationException(
                $"Adding product item for cart in '{shoppingCart.Status}' status is not allowed.");

        var pricedProductItem = productPriceCalculator.Calculate(productItem);

        return new ProductItemAddedToShoppingCart(
            cartId,
            pricedProductItem
        );
    }
}

public record RemoveProductItemFromShoppingCart(
    Guid ShoppingCartId,
    PricedProductItem ProductItem
)
{
    public static RemoveProductItemFromShoppingCart From(Guid? cartId, PricedProductItem? productItem)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (productItem == null)
            throw new ArgumentOutOfRangeException(nameof(productItem));

        return new RemoveProductItemFromShoppingCart(cartId.Value, productItem);
    }

    public static ProductItemRemovedFromShoppingCart Handle(
        RemoveProductItemFromShoppingCart command,
        ShoppingCart shoppingCart
    )
    {
        var (cartId, productItem) = command;

        if (shoppingCart.IsClosed)
            throw new InvalidOperationException(
                $"Adding product item for cart in '{shoppingCart.Status}' status is not allowed.");

        if (!shoppingCart.HasEnough(productItem))
            throw new InvalidOperationException("Not enough product items to remove");

        return new ProductItemRemovedFromShoppingCart(
            cartId,
            productItem
        );
    }
}

public record ConfirmShoppingCart(
    Guid ShoppingCartId
)
{
    public static ConfirmShoppingCart From(Guid? cartId)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new ConfirmShoppingCart(cartId.Value);
    }

    public static ShoppingCartConfirmed Handle(ConfirmShoppingCart command, ShoppingCart shoppingCart)
    {
        if (shoppingCart.IsClosed)
            throw new InvalidOperationException($"Confirming cart in '{shoppingCart.Status}' status is not allowed.");

        return new ShoppingCartConfirmed(
            shoppingCart.Id,
            DateTime.UtcNow
        );
    }
}

public record CancelShoppingCart(
    Guid ShoppingCartId
)
{
    public static CancelShoppingCart From(Guid? cartId)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new CancelShoppingCart(cartId.Value);
    }

    public static ShoppingCartCanceled Handle(CancelShoppingCart command, ShoppingCart shoppingCart)
    {
        if (shoppingCart.IsClosed)
            throw new InvalidOperationException($"Canceling cart in '{shoppingCart.Status}' status is not allowed.");

        return new ShoppingCartCanceled(
            shoppingCart.Id,
            DateTime.UtcNow
        );
    }
}

public interface IProductPriceCalculator
{
    PricedProductItem Calculate(ProductItem productItems);
}

public class FakeProductPriceCalculator: IProductPriceCalculator
{
    private readonly int value;

    private FakeProductPriceCalculator(int value)
    {
        this.value = value;
    }

    public static FakeProductPriceCalculator Returning(int value) => new(value);

    public PricedProductItem Calculate(ProductItem productItem)
    {
        var (productId, quantity) = productItem;

        return new PricedProductItem(productId, quantity, value);
    }
}
