using System;
using System.Collections.Generic;
using System.Linq;
using Carts.Carts.AddingProduct;
using Carts.Carts.ConfirmingCart;
using Carts.Carts.InitializingCart;
using Carts.Carts.Products;
using Carts.Carts.RemovingProduct;
using Core.Extensions;
using Core.Projections;

namespace Carts.Carts.GettingCartById;

public class CartDetails: IProjection
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }

    public CartStatus Status { get; set; }

    public IList<PricedProductItem> ProductItems { get; set; } = default!;

    public decimal TotalPrice => ProductItems.Sum(pi => pi.TotalPrice);

    public int Version { get; set; }


    public void When(object @event)
    {
        switch (@event)
        {
            case CartInitialized cartInitialized:
                Apply(cartInitialized);
                return;
            case ProductAdded cartInitialized:
                Apply(cartInitialized);
                return;
            case ProductRemoved cartInitialized:
                Apply(cartInitialized);
                return;
            case CartConfirmed cartInitialized:
                Apply(cartInitialized);
                return;
        }
    }

    public void Apply(CartInitialized @event)
    {
        Version++;

        Id = @event.CartId;
        ClientId = @event.ClientId;
        ProductItems = new List<PricedProductItem>();
        Status = @event.CartStatus;
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

    public void Apply(CartConfirmed @event)
    {
        Version++;

        Status = CartStatus.Confirmed;
    }

    private PricedProductItem? FindProductItemMatchingWith(PricedProductItem productItem)
    {
        return ProductItems
            .SingleOrDefault(pi => pi.MatchesProductAndPrice(productItem));
    }
}