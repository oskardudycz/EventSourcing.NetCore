using System;

namespace ECommerce.ShoppingCarts.GettingCartById
{
    public class ShoppingCartDetails
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public ShoppingCartStatus Status { get; set; }

        // public IList<PricedProductItem> ProductItems { get; set; } = default!;

        // public decimal TotalPrice => ProductItems.Sum(pi => pi.TotalPrice);
        public int Version { get; set; }
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
    }
}
