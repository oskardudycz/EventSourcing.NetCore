using Carts.Pricing;
using Carts.ShoppingCarts.AddingProduct;
using Carts.ShoppingCarts.CancelingCart;
using Carts.ShoppingCarts.ConfirmingCart;
using Carts.ShoppingCarts.OpeningCart;
using Carts.ShoppingCarts.Products;
using Carts.ShoppingCarts.RemovingProduct;
using Core.Aggregates;
using Core.Extensions;

namespace Carts.ShoppingCarts;

public class ShoppingCart: Aggregate
{
    public Guid ClientId { get; private set; }

    public ShoppingCartStatus Status { get; private set; }

    public IList<PricedProductItem> ProductItems { get; private set; } = default!;

    public decimal TotalPrice => ProductItems.Sum(pi => pi.TotalPrice);

    public static ShoppingCart Open(
        Guid cartId,
        Guid clientId)
    {
        return new ShoppingCart(cartId, clientId);
    }

    private ShoppingCart(){}

    public override void Evolve(object @event)
    {
        switch (@event)
        {
            case ShoppingCartOpened cartOpened:
                Apply(cartOpened);
                return;
            case ProductAdded cartOpened:
                Apply(cartOpened);
                return;
            case ProductRemoved cartOpened:
                Apply(cartOpened);
                return;
            case ShoppingCartConfirmed cartOpened:
                Apply(cartOpened);
                return;
            case ShoppingCartCanceled cartCanceled:
                Apply(cartCanceled);
                return;
        }
    }

    private ShoppingCart(
        Guid id,
        Guid clientId)
    {
        var @event = ShoppingCartOpened.Create(
            id,
            clientId,
            ShoppingCartStatus.Pending
        );

        Enqueue(@event);
        Apply(@event);
    }

    private void Apply(ShoppingCartOpened @event)
    {
        Version = 0;

        Id = @event.CartId;
        ClientId = @event.ClientId;
        ProductItems = new List<PricedProductItem>();
        Status = @event.ShoppingCartStatus;
    }

    public void AddProduct(
        IProductPriceCalculator productPriceCalculator,
        ProductItem productItem)
    {
        if(Status != ShoppingCartStatus.Pending)
            throw new InvalidOperationException($"Adding product for the cart in '{Status}' status is not allowed.");

        var pricedProductItem = productPriceCalculator.Calculate(productItem).Single();

        var @event = ProductAdded.Create(Id, pricedProductItem);

        Enqueue(@event);
        Apply(@event);
    }

    private void Apply(ProductAdded @event)
    {
        Version++;

        var newProductItem = @event.ProductItem;

        var existingProductItem = FindProductItemMatchingWith(newProductItem);

        if (existingProductItem is null)
        {
            ProductItems.Add(newProductItem);
            return;
        }

        ProductItems.Replace(
            existingProductItem,
            existingProductItem.MergeWith(newProductItem)
        );
    }

    public void RemoveProduct(
        PricedProductItem productItemToBeRemoved)
    {
        if(Status != ShoppingCartStatus.Pending)
            throw new InvalidOperationException($"Removing product from the cart in '{Status}' status is not allowed.");

        var existingProductItem = FindProductItemMatchingWith(productItemToBeRemoved);

        if (existingProductItem is null)
            throw new InvalidOperationException($"Product with id `{productItemToBeRemoved.ProductId}` and price '{productItemToBeRemoved.UnitPrice}' was not found in cart.");

        if(!existingProductItem.HasEnough(productItemToBeRemoved.Quantity))
            throw new InvalidOperationException($"Cannot remove {productItemToBeRemoved.Quantity} items of Product with id `{productItemToBeRemoved.ProductId}` as there are only ${existingProductItem.Quantity} items in card");

        var @event = ProductRemoved.Create(Id, productItemToBeRemoved);

        Enqueue(@event);
        Apply(@event);
    }

    private void Apply(ProductRemoved @event)
    {
        Version++;

        var productItemToBeRemoved = @event.ProductItem;

        var existingProductItem = FindProductItemMatchingWith(@event.ProductItem);

        if (existingProductItem == null)
            return;

        if (existingProductItem.HasTheSameQuantity(productItemToBeRemoved))
        {
            ProductItems.Remove(existingProductItem);
            return;
        }

        ProductItems.Replace(
            existingProductItem,
            existingProductItem.Substract(productItemToBeRemoved)
        );
    }

    public void Confirm()
    {
        if(Status != ShoppingCartStatus.Pending)
            throw new InvalidOperationException($"Confirming cart in '{Status}' status is not allowed.");

        var @event = ShoppingCartConfirmed.Create(Id, DateTime.UtcNow);

        Enqueue(@event);
        Apply(@event);
    }

    private void Apply(ShoppingCartConfirmed @event)
    {
        Version++;

        Status = ShoppingCartStatus.Confirmed;
    }

    public void Cancel()
    {
        if(Status != ShoppingCartStatus.Pending)
            throw new InvalidOperationException($"Confirming cart in '{Status}' status is not allowed.");

        var @event = ShoppingCartCanceled.Create(Id, DateTime.UtcNow);

        Enqueue(@event);
        Apply(@event);
    }

    private void Apply(ShoppingCartCanceled @event)
    {
        Version++;

        Status = ShoppingCartStatus.Canceled;
    }

    private PricedProductItem? FindProductItemMatchingWith(PricedProductItem productItem)
    {
        return ProductItems
            .SingleOrDefault(pi => pi.MatchesProductAndPrice(productItem));
    }
}
