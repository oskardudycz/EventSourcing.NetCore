using System;
using System.Collections.Generic;
using System.Linq;

namespace ECommerce.ShoppingCarts.GettingCartById
{
    public class ShoppingCartDetails
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public ShoppingCartStatus Status { get; set; }
        public List<ShoppingCartDetailsProductItem> ProductItems { get; set; } = default!;
        public int Version { get; set; }
    }

    public class ShoppingCartDetailsProductItem
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public static class ShoppingCartDetailsProjection
    {
        public static ShoppingCartDetails Handle(ShoppingCartInitialized @event)
        {
            var (shoppingCartId, clientId, shoppingCartStatus) = @event;

            return new ShoppingCartDetails
            {
                Id = shoppingCartId,
                ClientId = clientId,
                Status = shoppingCartStatus,
                Version = 1
            };
        }

        public static void Handle(ShoppingCartConfirmed @event, ShoppingCartDetails view)
        {
            view.Status = ShoppingCartStatus.Confirmed;
            view.Version++;
        }

        public static void Handle(ProductItemAddedToShoppingCart @event, ShoppingCartDetails view)
        {
            var productItem = @event.ProductItem;
            var existingProductItem = view.ProductItems
                .FirstOrDefault(x => x.ProductId == @event.ProductItem.ProductId);

            if (existingProductItem == null)
            {
                view.ProductItems.Add(new ShoppingCartDetailsProductItem
                {
                    ProductId = productItem.ProductId,
                    Quantity = productItem.Quantity,
                    UnitPrice = productItem.UnitPrice
                });
            }
            else
            {
                existingProductItem.Quantity += productItem.Quantity;
            }

            view.Version++;
        }

        public static void Handle(ProductItemRemovedFromShoppingCart @event, ShoppingCartDetails view)
        {
            var productItem = @event.ProductItem;
            var existingProductItem = view.ProductItems
                .Single(x => x.ProductId == @event.ProductItem.ProductId);

            if (existingProductItem.Quantity == productItem.Quantity)
            {
                view.ProductItems.Remove(existingProductItem);
            }
            else
            {
                existingProductItem.Quantity -= productItem.Quantity;
            }

            view.Version++;
        }
    }
}
