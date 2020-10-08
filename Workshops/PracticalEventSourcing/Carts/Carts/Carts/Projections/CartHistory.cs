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

    public class CartHistoryTransformation :
        ITransform<CartInitialized, CartHistory>,
        ITransform<ProductAdded, CartHistory>,
        ITransform<ProductRemoved, CartHistory>,
        ITransform<CartConfirmed, CartHistory>
    {
        public CartHistory Transform(EventStream stream, Event<CartInitialized> input)
        {
            return new CartHistory
            {
                Id = Guid.NewGuid(),
                CartId = input.Data.CartId,
                Description = $"Created tentative Cart with id {input.Data.CartId}"
            };
        }

        public CartHistory Transform(EventStream stream, Event<ProductAdded> input)
        {
            return new CartHistory
            {
                Id = Guid.NewGuid(),
                CartId = input.Data.CartId,
                Description = $"Added {input.Data.ProductItem.Quantity} Product with id `{input.Data.ProductItem.ProductId}` to Cart `{input.Data.CartId}`"
            };
        }

        public CartHistory Transform(EventStream stream, Event<ProductRemoved> input)
        {
            return new CartHistory
            {
                Id = Guid.NewGuid(),
                CartId = input.Data.CartId,
                Description = $"Removed Product {input.Data.ProductItem.Quantity} with id `{input.Data.ProductItem.ProductId}` to Cart `{input.Data.CartId}`"
            };
        }

        public CartHistory Transform(EventStream stream, Event<CartConfirmed> input)
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
