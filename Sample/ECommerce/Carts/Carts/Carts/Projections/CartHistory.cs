using System;
using Carts.Carts.Events;
using Marten.Events;
using Marten.Events.Projections;

namespace Carts.Carts.Projections
{
    public class CartHistory
    {
        public Guid Id { get; set; }
        public Guid CartId { get; set; }
        public string Description { get; set; }
    }

    public class CartHistoryTransformation : EventProjection
    {
        public CartHistory Transform(IEvent<CartInitialized> input)
        {
            return new CartHistory
            {
                Id = Guid.NewGuid(),
                CartId = input.Data.CartId,
                Description = $"Created tentative Cart with id {input.Data.CartId}"
            };
        }

        public CartHistory Transform(IEvent<ProductAdded> input)
        {
            return new CartHistory
            {
                Id = Guid.NewGuid(),
                CartId = input.Data.CartId,
                Description = $"Added {input.Data.ProductItem.Quantity} Product with id `{input.Data.ProductItem.ProductId}` to Cart `{input.Data.CartId}`"
            };
        }

        public CartHistory Transform(IEvent<ProductRemoved> input)
        {
            return new CartHistory
            {
                Id = Guid.NewGuid(),
                CartId = input.Data.CartId,
                Description = $"Removed Product {input.Data.ProductItem.Quantity} with id `{input.Data.ProductItem.ProductId}` to Cart `{input.Data.CartId}`"
            };
        }

        public CartHistory Transform(IEvent<CartConfirmed> input)
        {
            return new CartHistory
            {
                Id = Guid.NewGuid(),
                CartId = input.Data.CartId,
                Description = $"Confirmed Cart with id `{input.Data.CartId}`"
            };
        }
    }
}
