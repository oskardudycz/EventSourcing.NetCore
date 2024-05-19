using ECommerce.Domain.Pricing;
using ECommerce.Domain.ShoppingCarts.AddingProduct;
using ECommerce.Domain.ShoppingCarts.CancelingCart;
using ECommerce.Domain.ShoppingCarts.ConfirmingCart;
using ECommerce.Domain.ShoppingCarts.OpeningCart;
using ECommerce.Domain.ShoppingCarts.Products;
using ECommerce.Domain.ShoppingCarts.RemovingProduct;

namespace ECommerce.Domain.ShoppingCarts;

public class ShoppingCart
{
    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }
    public ShoppingCartStatus Status { get; private set; }
    public IList<PricedProductItem> ProductItems { get; private set; } = default!;

    public decimal TotalPrice => ProductItems.Sum(pi => pi.TotalPrice);

    public static (ShoppingCart, ShoppingCartOpened) Open(
        Guid cartId,
        Guid clientId)
    {

        var @event = new ShoppingCartOpened(
            cartId,
            clientId
        );
        var shoppingCart = new ShoppingCart();
        shoppingCart.Apply(@event);

        return (shoppingCart, @event);
    }

    private ShoppingCart() { }

    private void Apply(ShoppingCartOpened @event)
    {
        Id = @event.CartId;
        ClientId = @event.ClientId;
        ProductItems = new List<PricedProductItem>();
        Status = ShoppingCartStatus.Pending;
    }

    public ProductAdded AddProduct(
        IProductPriceCalculator productPriceCalculator,
        ProductItem productItem)
    {
        if (Status != ShoppingCartStatus.Pending)
            throw new InvalidOperationException($"Adding product for the cart in '{Status}' status is not allowed.");

        var pricedProductItem = productPriceCalculator.Calculate(productItem).Single();

        var @event = new ProductAdded(Id, pricedProductItem);
        Apply(@event);
        return @event;
    }

    private void Apply(ProductAdded @event)
    {
        var newProductItem = @event.ProductItem;

        var existingProductItem = FindProductItemMatchingWith(newProductItem);

        if (existingProductItem is null)
        {
            ProductItems.Add(newProductItem);
            return;
        }

        ProductItems[ProductItems.IndexOf(existingProductItem)] =
            existingProductItem.MergeWith(newProductItem);
    }

    public ProductRemoved RemoveProduct(
        PricedProductItem productItemToBeRemoved)
    {
        if (Status != ShoppingCartStatus.Pending)
            throw new InvalidOperationException($"Removing product from the cart in '{Status}' status is not allowed.");

        var existingProductItem = FindProductItemMatchingWith(productItemToBeRemoved);

        if (existingProductItem is null)
            throw new InvalidOperationException(
                $"Product with id `{productItemToBeRemoved.ProductId}` and price '{productItemToBeRemoved.UnitPrice}' was not found in cart.");

        if (!existingProductItem.HasEnough(productItemToBeRemoved.Quantity))
            throw new InvalidOperationException(
                $"Cannot remove {productItemToBeRemoved.Quantity} items of Product with id `{productItemToBeRemoved.ProductId}` as there are only ${existingProductItem.Quantity} items in card");

        var @event = new ProductRemoved(Id, productItemToBeRemoved);
        Apply(@event);
        return @event;
    }

    private void Apply(ProductRemoved @event)
    {
        var productItemToBeRemoved = @event.ProductItem;

        var existingProductItem = FindProductItemMatchingWith(@event.ProductItem);

        if (existingProductItem == null)
            return;

        if (existingProductItem.HasTheSameQuantity(productItemToBeRemoved))
        {
            ProductItems.Remove(existingProductItem);
            return;
        }

        ProductItems[ProductItems.IndexOf(existingProductItem)] =
            existingProductItem.Subtract(productItemToBeRemoved);
    }

    public ShoppingCartConfirmed Confirm()
    {
        if (Status != ShoppingCartStatus.Pending)
            throw new InvalidOperationException($"Confirming cart in '{Status}' status is not allowed.");

        var @event = new ShoppingCartConfirmed(Id, DateTime.UtcNow);
        Apply(@event);
        return @event;
    }

    private void Apply(ShoppingCartConfirmed @event)
    {
        Status = ShoppingCartStatus.Confirmed;
    }

    public ShoppingCartCanceled Cancel()
    {
        if (Status != ShoppingCartStatus.Pending)
            throw new InvalidOperationException($"Canceling cart in '{Status}' status is not allowed.");

        var @event = new ShoppingCartCanceled(Id, DateTime.UtcNow);
        Apply(@event);
        return @event;
    }

    private void Apply(ShoppingCartCanceled @event) =>
        Status = ShoppingCartStatus.Canceled;

    private PricedProductItem? FindProductItemMatchingWith(PricedProductItem productItem) =>
        ProductItems
            .SingleOrDefault(pi => pi.MatchesProductAndPrice(productItem));
}
