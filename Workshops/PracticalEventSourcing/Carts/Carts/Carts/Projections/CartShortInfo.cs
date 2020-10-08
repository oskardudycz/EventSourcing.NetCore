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

    internal class CartShortInfoProjection : ViewProjection<CartShortInfo, Guid>
    {
        public CartShortInfoProjection()
        {
            ProjectEvent<CartInitialized>(@event => @event.CartId,
                (item, @event) => item.Apply(@event));

            ProjectEvent<ProductAdded>(@event => @event.CartId,
                (item, @event) => item.Apply(@event));

            ProjectEvent<ProductRemoved>(@event => @event.CartId,
                (item, @event) => item.Apply(@event));

            ProjectEvent<CartConfirmed>(@event => @event.CartId,
                (item, @event) => item.Apply(@event));
        }
    }
}
