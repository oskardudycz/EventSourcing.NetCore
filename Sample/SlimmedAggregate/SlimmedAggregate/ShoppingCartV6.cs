namespace SlimmedAggregate.V6;

public record ShoppingCart(
    Guid Id,
    bool IsOpened,
    Dictionary<Guid, int> ProductItems
)
{
    public static ShoppingCart Create(ShoppingCartOpened @event) =>
        new ShoppingCart(
            @event.CartId,
            true,
            new Dictionary<Guid, int>()
        );


    public ShoppingCart Apply(ProductItemAdded @event) =>
        this with
        {
            ProductItems = ProductItems
                .Select(p =>
                    p.Key != @event.ProductItem.ProductId
                        ? p
                        : new KeyValuePair<Guid, int>(
                            @event.ProductItem.ProductId,
                            p.Value + @event.ProductItem.Quantity
                        )
                )
                .ToDictionary(ks => ks.Key, vs => vs.Value)
        };

    public ShoppingCart Apply(ProductItemRemoved @event) =>
        this with
        {
            ProductItems = ProductItems
                .Select(p =>
                    p.Key != @event.ProductItem.ProductId
                        ? p
                        : new KeyValuePair<Guid, int>(
                            @event.ProductItem.ProductId,
                            p.Value - @event.ProductItem.Quantity
                        )
                )
                .ToDictionary(ks => ks.Key, vs => vs.Value)
        };

    public ShoppingCart Apply(ShoppingCartConfirmed @event) =>
        this with { IsOpened = false };

    public ShoppingCart Apply(ShoppingCartCanceled @event) =>
        this with { IsOpened = false };
}

public static class ShoppingCartService
{
    public static ShoppingCartOpened Open(
        Guid id,
        Guid clientId
    )
    {
        return new ShoppingCartOpened(
            id,
            clientId
        );
    }

    public static ProductItemAdded AddProduct(ShoppingCart shoppingCart, ProductItem productItem)
    {
        if (!shoppingCart.IsOpened)
            throw new InvalidOperationException("Adding product to the closed cart is not allowed.");

        return new ProductItemAdded(shoppingCart.Id, productItem);
    }

    public static ProductItemRemoved RemoveProduct(ShoppingCart shoppingCart, ProductItem productItemToBeRemoved)
    {
        if (!shoppingCart.IsOpened)
            throw new InvalidOperationException("Removing product to the closed cart is not allowed.");

        if (!shoppingCart.ProductItems.TryGetValue(productItemToBeRemoved.ProductId, out var currentQuantity))
            throw new InvalidOperationException(
                $"Product with id `{productItemToBeRemoved.ProductId}` and price was not found in cart.");

        if (currentQuantity < productItemToBeRemoved.Quantity)
            throw new InvalidOperationException(
                $"Cannot remove {productItemToBeRemoved.Quantity} items of Product with id `{productItemToBeRemoved.ProductId}` as there are only ${currentQuantity} items in card");

        return new ProductItemRemoved(shoppingCart.Id, productItemToBeRemoved);
    }

    public static ShoppingCartConfirmed Confirm(ShoppingCart shoppingCart)
    {
        if (!shoppingCart.IsOpened)
            throw new InvalidOperationException("Confirming closed cart is not allowed.");

        return new ShoppingCartConfirmed(shoppingCart.Id, DateTime.UtcNow);
    }

    public static ShoppingCartCanceled Cancel(ShoppingCart shoppingCart)
    {
        if (!shoppingCart.IsOpened)
            throw new InvalidOperationException("Canceling closed cart is not allowed.");

        return new ShoppingCartCanceled(shoppingCart.Id, DateTime.UtcNow);
    }
}
