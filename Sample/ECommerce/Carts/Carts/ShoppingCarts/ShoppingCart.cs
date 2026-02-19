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
    public Guid ClientId { get; set; }

    public ShoppingCartStatus Status { get; set; }

    public IList<PricedProductItem> ProductItems { get; set; } = new List<PricedProductItem>();

    public decimal TotalPrice => ProductItems.Sum(pi => pi.TotalPrice);

    public static ShoppingCart Open(Guid cartId, Guid clientId)
    {
        var shoppingCart = new ShoppingCart();

        shoppingCart.Enqueue(ShoppingCartOpened.Create(cartId, clientId));

        return shoppingCart;
    }

    public void AddProduct(
        IProductPriceCalculator productPriceCalculator,
        ProductItem productItem)
    {
        if(Status != ShoppingCartStatus.Pending)
            throw new InvalidOperationException($"Adding product for the cart in '{Status}' status is not allowed.");

        var pricedProductItem = productPriceCalculator.Calculate(productItem).Single();

        Enqueue(ProductAdded.Create(Id, pricedProductItem));
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

        Enqueue(ProductRemoved.Create(Id, productItemToBeRemoved));
    }

    public void Confirm()
    {
        if(Status != ShoppingCartStatus.Pending)
            throw new InvalidOperationException($"Confirming cart in '{Status}' status is not allowed.");

        if (ProductItems.Count == 0)
            throw new InvalidOperationException($"Confirming empty cart is not allowed.");

        Enqueue(ShoppingCartConfirmed.Create(Id, DateTime.UtcNow));
    }

    public void Cancel()
    {
        if(Status != ShoppingCartStatus.Pending)
            throw new InvalidOperationException($"Canceling cart in '{Status}' status is not allowed.");

        Enqueue(ShoppingCartCanceled.Create(Id, DateTime.UtcNow));
    }

    public override void Apply(object @event)
    {
        switch (@event)
        {
            case ShoppingCartOpened opened:
                Id = opened.CartId;
                ClientId = opened.ClientId;
                ProductItems = new List<PricedProductItem>();
                Status = ShoppingCartStatus.Pending;
                break;
            case ProductAdded productAdded:
                var newProductItem = productAdded.ProductItem;
                var existing = FindProductItemMatchingWith(newProductItem);
                if (existing is null)
                    ProductItems.Add(newProductItem);
                else
                    ProductItems.Replace(existing, existing.MergeWith(newProductItem));
                break;
            case ProductRemoved productRemoved:
                var toRemove = productRemoved.ProductItem;
                var found = FindProductItemMatchingWith(toRemove);
                if (found == null) break;
                if (found.HasTheSameQuantity(toRemove))
                    ProductItems.Remove(found);
                else
                    ProductItems.Replace(found, found.Subtract(toRemove));
                break;
            case ShoppingCartConfirmed:
                Status = ShoppingCartStatus.Confirmed;
                break;
            case ShoppingCartCanceled:
                Status = ShoppingCartStatus.Canceled;
                break;
        }
    }

    private PricedProductItem? FindProductItemMatchingWith(PricedProductItem productItem) =>
        ProductItems
            .SingleOrDefault(pi => pi.MatchesProductAndPrice(productItem));
}
