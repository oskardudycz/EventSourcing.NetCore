using Carts.ShoppingCarts.AddingProduct;
using Carts.ShoppingCarts.CancelingCart;
using Carts.ShoppingCarts.ConfirmingCart;
using Carts.ShoppingCarts.OpeningCart;
using Carts.ShoppingCarts.Products;
using Carts.ShoppingCarts.RemovingProduct;
using Core.Extensions;
using Core.Projections;

namespace Carts.ShoppingCarts.GettingCartById;

public class ShoppingCartDetails: IVersionedProjection
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }

    public ShoppingCartStatus Status { get; set; }

    public IList<PricedProductItem> ProductItems { get; set; } = default!;

    public decimal TotalPrice => ProductItems.Sum(pi => pi.TotalPrice);

    public long Version { get; set; }


    public void Apply(object @event)
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

    public void Apply(ShoppingCartOpened @event)
    {
        Id = @event.CartId;
        ClientId = @event.ClientId;
        ProductItems = new List<PricedProductItem>();
        Status = @event.ShoppingCartStatus;
        Version = 0;
    }

    public void Apply(ProductAdded @event)
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

    public void Apply(ProductRemoved @event)
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

    public void Apply(ShoppingCartConfirmed @event)
    {
        Version++;

        Status = ShoppingCartStatus.Confirmed;
    }

    public void Apply(ShoppingCartCanceled @event)
    {
        Version++;

        Status = ShoppingCartStatus.Canceled;
    }

    private PricedProductItem? FindProductItemMatchingWith(PricedProductItem productItem)
    {
        return ProductItems
            .SingleOrDefault(pi => pi.MatchesProductAndPrice(productItem));
    }

    public ulong LastProcessedPosition { get; set; }
}
