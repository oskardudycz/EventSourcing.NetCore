using Carts.ShoppingCarts.AddingProduct;
using Carts.ShoppingCarts.CancelingCart;
using Carts.ShoppingCarts.ConfirmingCart;
using Carts.ShoppingCarts.OpeningCart;
using Carts.ShoppingCarts.Products;
using Carts.ShoppingCarts.RemovingProduct;
using Core.Extensions;
using Marten.Events.Aggregation;

namespace Carts.ShoppingCarts.GettingCartById;

public class ShoppingCartDetails
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }

    public ShoppingCartStatus Status { get; set; }

    public IList<PricedProductItem> ProductItems { get; set; } = default!;

    public decimal TotalPrice => ProductItems.Sum(pi => pi.TotalPrice);

    public int Version { get; set; }

    public void Apply(ShoppingCartOpened @event)
    {
        Id = @event.CartId;
        ClientId = @event.ClientId;
        ProductItems = new List<PricedProductItem>();
        Status = ShoppingCartStatus.Pending;
    }

    public void Apply(ProductAdded @event)
    {
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
        var productItemToBeRemoved = @event.ProductItem;

        var existingProductItem = FindProductItemMatchingWith(@event.ProductItem);

        if(existingProductItem == null)
            return;

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

    public void Apply(ShoppingCartConfirmed @event) =>
        Status = ShoppingCartStatus.Confirmed;

    public void Apply(ShoppingCartCanceled @event) =>
        Status = ShoppingCartStatus.Canceled;

    private PricedProductItem? FindProductItemMatchingWith(PricedProductItem productItem) =>
        ProductItems
            .SingleOrDefault(pi => pi.MatchesProductAndPrice(productItem));
}

public class CartDetailsProjection : SingleStreamProjection<ShoppingCartDetails>
{
    public CartDetailsProjection()
    {
        ProjectEvent<ShoppingCartOpened>((item, @event) => item.Apply(@event));

        ProjectEvent<ProductAdded>((item, @event) => item.Apply(@event));

        ProjectEvent<ProductRemoved>((item, @event) => item.Apply(@event));

        ProjectEvent<ShoppingCartConfirmed>((item, @event) => item.Apply(@event));

        ProjectEvent<ShoppingCartCanceled>((item, @event) => item.Apply(@event));
    }
}
