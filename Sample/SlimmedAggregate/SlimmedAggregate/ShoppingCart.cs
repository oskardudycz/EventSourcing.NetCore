namespace SlimmedAggregate;

public class ShoppingCart: Aggregate
{
    public Guid ClientId { get; private set; }

    public ShoppingCartStatus Status { get; private set; }

    public IList<ProductItem> ProductItems { get; private set; } = default!;

    public static ShoppingCart Open(
        Guid cartId,
        Guid clientId
    )
    {
        return new ShoppingCart(cartId, clientId);
    }

    private ShoppingCart() { }

    private ShoppingCart(
        Guid id,
        Guid clientId)
    {
        var @event = new ShoppingCartOpened(
            id,
            clientId
        );

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(ShoppingCartOpened @event)
    {
        Id = @event.CartId;
        ClientId = @event.ClientId;
        ProductItems = new List<ProductItem>();
        Status = ShoppingCartStatus.Pending;
    }

    public void AddProduct(ProductItem productItem)
    {
        if (Status != ShoppingCartStatus.Pending)
            throw new InvalidOperationException($"Adding product for the cart in '{Status}' status is not allowed.");

        var @event = new ProductItemAdded(Id, productItem);

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(ProductItemAdded @event)
    {
        var newProductItem = @event.ProductItem;

        var existingProductItem = ProductItems
            .SingleOrDefault(pi => pi.ProductId == newProductItem.ProductId);

        if (existingProductItem is null)
        {
            ProductItems.Add(newProductItem);
            return;
        }

        ProductItems[ProductItems.IndexOf(existingProductItem)] =
            existingProductItem with { Quantity = existingProductItem.Quantity + newProductItem.Quantity };
    }

    public void RemoveProduct(ProductItem productItemToBeRemoved)
    {
        if (Status != ShoppingCartStatus.Pending)
            throw new InvalidOperationException($"Removing product from the cart in '{Status}' status is not allowed.");

        var existingProductItem = ProductItems
            .SingleOrDefault(pi => pi.ProductId == productItemToBeRemoved.ProductId);

        if (existingProductItem is null)
            throw new InvalidOperationException(
                $"Product with id `{productItemToBeRemoved.ProductId}` and price was not found in cart.");

        if (existingProductItem.Quantity < productItemToBeRemoved.Quantity)
            throw new InvalidOperationException(
                $"Cannot remove {productItemToBeRemoved.Quantity} items of Product with id `{productItemToBeRemoved.ProductId}` as there are only ${existingProductItem.Quantity} items in card");

        var @event = new ProductItemRemoved(Id, productItemToBeRemoved);

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(ProductItemRemoved @event)
    {
        var productItemToBeRemoved = @event.ProductItem;

        var existingProductItem = ProductItems
            .SingleOrDefault(pi => pi.ProductId == productItemToBeRemoved.ProductId);

        if (existingProductItem == null)
            return;

        if (existingProductItem == productItemToBeRemoved)
        {
            ProductItems.Remove(existingProductItem);
            return;
        }

        ProductItems[ProductItems.IndexOf(existingProductItem)] =
            existingProductItem with { Quantity = existingProductItem.Quantity - productItemToBeRemoved.Quantity };
    }

    public void Confirm()
    {
        if (Status != ShoppingCartStatus.Pending)
            throw new InvalidOperationException($"Confirming cart in '{Status}' status is not allowed.");

        var @event = new ShoppingCartConfirmed(Id, DateTime.UtcNow);

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(ShoppingCartConfirmed @event)
    {
        Status = ShoppingCartStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status != ShoppingCartStatus.Pending)
            throw new InvalidOperationException($"Canceling cart in '{Status}' status is not allowed.");

        var @event = new ShoppingCartCanceled(Id, DateTime.UtcNow);

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(ShoppingCartCanceled @event)
    {
        Status = ShoppingCartStatus.Canceled;
    }
}
