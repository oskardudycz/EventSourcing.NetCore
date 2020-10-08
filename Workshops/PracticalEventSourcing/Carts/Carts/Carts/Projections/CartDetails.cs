using System;
using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;
using Carts.Carts.Events;
using Carts.Carts.ValueObjects;
using Carts.Pricing;
using Core.Extensions;

namespace Carts.Carts.Projections
{
    public class CartDetails
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }

        public CartStatus Status { get; set; }

        public IList<PricedProductItem> ProductItems { get; set; }

        public decimal TotalPrice => ProductItems.Sum(pi => pi.TotalPrice);

        public int Version { get; set; }

        private void Apply(CartInitialized @event)
        {
            Version++;

            Id = @event.CartId;
            ClientId = @event.ClientId;
            ProductItems = new List<PricedProductItem>();
            Status = @event.CartStatus;
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

        private void Apply(ProductRemoved @event)
        {
            Version++;

            var productItemToBeRemoved = @event.ProductItem;

            var existingProductItem = FindProductItemMatchingWith(@event.ProductItem);

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

        private void Apply(CartConfirmed @event)
        {
            Version++;

            Status = CartStatus.Confirmed;
        }

        private PricedProductItem FindProductItemMatchingWith(PricedProductItem productItem)
        {
            return ProductItems
                .SingleOrDefault(pi => pi.MatchesProductAndPrice(productItem));
        }
    }
}
