using System;
using Carts.Carts.Events;
using Marten.Events.Projections;

namespace Carts.Carts.Projections
{
    public class CartShortInfo
    {
        public Guid Id { get; set; }

        public int TotalItemsCount { get; set; }

        public CartStatus Status { get; set; }

        public void Apply(CartInitialized @event)
        {
            Id = @event.CartId;
            TotalItemsCount = 0;
            Status = CartStatus.Pending;
        }

        public void Apply(ProductAdded @event)
        {
            TotalItemsCount += @event.ProductItem.Quantity;
        }

        public void Apply(ProductRemoved @event)
        {
            TotalItemsCount -= @event.ProductItem.Quantity;
        }

        public void Apply(CartConfirmed @event)
        {
            Status = CartStatus.Confirmed;
        }
    }
}
