using System;
using System.Collections.Generic;
using System.Linq;
using Carts.ShoppingCarts.AddingProduct;
using Carts.ShoppingCarts.ConfirmingCart;
using Carts.ShoppingCarts.InitializingCart;
using Carts.ShoppingCarts.Products;
using Carts.ShoppingCarts.RemovingProduct;
using Core.Extensions;
using Core.Projections;

namespace Carts.ShoppingCarts.GettingCartById;

public class ShoppingCartDetails: IProjection
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }

    public ShoppingCartStatus Status { get; set; }

    public IList<PricedProductItem> ProductItems { get; set; } = default!;

    public decimal TotalPrice => ProductItems.Sum(pi => pi.TotalPrice);

    public int Version { get; set; }


    public void When(object @event)
    {
        switch (@event)
        {
            case ShoppingCartInitialized cartInitialized:
                Apply(cartInitialized);
                return;
            case ProductAdded cartInitialized:
                Apply(cartInitialized);
                return;
            case ProductRemoved cartInitialized:
                Apply(cartInitialized);
                return;
            case ShoppingCartConfirmed cartInitialized:
                Apply(cartInitialized);
                return;
        }
    }

    public void Apply(ShoppingCartInitialized @event)
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

    private PricedProductItem? FindProductItemMatchingWith(PricedProductItem productItem)
    {
        return ProductItems
            .SingleOrDefault(pi => pi.MatchesProductAndPrice(productItem));
    }
}
