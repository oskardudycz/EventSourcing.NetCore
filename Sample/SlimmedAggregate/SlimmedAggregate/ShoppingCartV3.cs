namespace SlimmedAggregate.V3;

public class ShoppingCart
{
    public Guid Id { get; private set; }
    public bool IsOpened { get; private set; }

    public Dictionary<Guid, int> ProductItems { get; private set; } = default!;

    private ShoppingCart() { }

    public static (ShoppingCart, ShoppingCartOpened) Open(
        Guid id,
        Guid clientId
    )
    {
        var @event = new ShoppingCartOpened(
            id,
            clientId
        );

        var shoppingCart = new ShoppingCart();
        shoppingCart.Apply(@event);

        return (shoppingCart, @event);
    }

    public void Apply(ShoppingCartOpened @event)
    {
        Id = @event.CartId;
        ProductItems = new Dictionary<Guid, int>();
        IsOpened = true;
    }

    public ProductItemAdded AddProduct(ProductItem productItem)
    {
        if (!IsOpened)
            throw new InvalidOperationException("Adding product to the closed cart is not allowed.");

        var @event = new ProductItemAdded(Id, productItem);

        Apply(@event);

        return @event;
    }

    public void Apply(ProductItemAdded @event)
    {
        var newProductItem = @event.ProductItem;

        ProductItems[newProductItem.ProductId] =
            ProductItems.GetValueOrDefault(newProductItem.ProductId, 0) + newProductItem.Quantity;
    }

    public ProductItemRemoved RemoveProduct(ProductItem productItemToBeRemoved)
    {
        if (!IsOpened)
            throw new InvalidOperationException("Removing product to the closed cart is not allowed.");

        if (!ProductItems.TryGetValue(productItemToBeRemoved.ProductId, out var currentQuantity))
            throw new InvalidOperationException(
                $"Product with id `{productItemToBeRemoved.ProductId}` and price was not found in cart.");

        if (currentQuantity < productItemToBeRemoved.Quantity)
            throw new InvalidOperationException(
                $"Cannot remove {productItemToBeRemoved.Quantity} items of Product with id `{productItemToBeRemoved.ProductId}` as there are only ${currentQuantity} items in card");

        var @event = new ProductItemRemoved(Id, productItemToBeRemoved);

        Apply(@event);

        return @event;
    }

    public void Apply(ProductItemRemoved @event)
    {
        var productItemToBeRemoved = @event.ProductItem;

        if (!ProductItems.TryGetValue(productItemToBeRemoved.ProductId, out var currentQuantity) ||
            currentQuantity < productItemToBeRemoved.Quantity)
            return;


        ProductItems[productItemToBeRemoved.ProductId] =
            currentQuantity - productItemToBeRemoved.Quantity;
    }

    public ShoppingCartConfirmed Confirm()
    {
        if (!IsOpened)
            throw new InvalidOperationException("Confirming closed cart is not allowed.");

        var @event = new ShoppingCartConfirmed(Id, DateTime.UtcNow);

        Apply(@event);

        return @event;
    }

    public void Apply(ShoppingCartConfirmed @event) =>
        IsOpened = false;

    public ShoppingCartCanceled Cancel()
    {
        if (!IsOpened)
            throw new InvalidOperationException("Canceling closed cart is not allowed.");

        var @event = new ShoppingCartCanceled(Id, DateTime.UtcNow);

        Apply(@event);

        return @event;
    }

    public void Apply(ShoppingCartCanceled @event) =>
        IsOpened = false;
}
