using System;
using Core.Aggregates;
using Carts.Carts;

namespace Carts.Tests.Builders
{
    internal class CartBuilder
    {
        private Func<Cart> build  = () => new Cart();

        public CartBuilder Initialized()
        {
            var cartId = Guid.NewGuid();
            var clientId = Guid.NewGuid();

            // When
            var cart = Cart.Initialize(
                cartId,
                clientId
            );

            build = () => cart;

            return this;
        }

        public static CartBuilder Create() => new();

        public Cart Build()
        {
            var cart = build();
            ((IAggregate)cart).DequeueUncommittedEvents();
            return cart;
        }
    }
}
