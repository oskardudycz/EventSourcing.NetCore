using Carts.Pricing;
using Carts.ShoppingCarts.Products;
using Core.Extensions;

namespace Carts;

public class ShoppingCart
{
    public Guid Id { get; private set; }

    public Guid ClientId { get; private set; }

    public ShoppingCartStatus Status { get; private set; }

    public IList<PricedProductItem> ProductItems { get; private set; } = default!;

    public Money TotalPrice => new Money( ProductItems.Sum(pi => pi.TotalPrice));

    public static ShoppingCart Open(
        Guid cartId,
        Guid clientId)
    {
        return new ShoppingCart(cartId, clientId);
    }

    public ShoppingCart() { }

    private ShoppingCart(
        Guid id,
        Guid clientId)
    {
        Id = id;
        ClientId = clientId;
        ProductItems = new List<PricedProductItem>();
        Status = ShoppingCartStatus.Pending;
    }

    public void AddProduct(
        IProductPriceCalculator productPriceCalculator,
        ProductItem productItem)
    {
        if (Status != ShoppingCartStatus.Pending)
            throw new InvalidOperationException($"Adding product for the cart in '{Status}' status is not allowed.");

        var pricedProductItem = productPriceCalculator.Calculate(productItem).Single();

        var existingProductItem = FindProductItemMatchingWith(pricedProductItem);

        if (existingProductItem is null)
        {
            ProductItems.Add(pricedProductItem);
            return;
        }

        ProductItems.Replace(
            existingProductItem,
            existingProductItem.MergeWith(pricedProductItem)
        );
    }


    public void RemoveProduct(
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


        if (existingProductItem.HasTheSameQuantity(productItemToBeRemoved))
        {
            ProductItems.Remove(existingProductItem);
            return;
        }

        ProductItems.Replace(
            existingProductItem,
            existingProductItem.Subtract(productItemToBeRemoved)
        );
    }

    public void Confirm()
    {
        if (Status != ShoppingCartStatus.Pending)
            throw new InvalidOperationException($"Confirming cart in '{Status}' status is not allowed.");

        Status = ShoppingCartStatus.Confirmed;
    }


    public void Cancel()
    {
        if (Status != ShoppingCartStatus.Pending)
            throw new InvalidOperationException($"Canceling cart in '{Status}' status is not allowed.");


        Status = ShoppingCartStatus.Canceled;
    }

    private PricedProductItem? FindProductItemMatchingWith(PricedProductItem productItem)
    {
        return ProductItems
            .SingleOrDefault(pi => pi.MatchesProductAndPrice(productItem));
    }
}
