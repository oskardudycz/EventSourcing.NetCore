using System;
using Ardalis.GuardClauses;
using Carts.Carts.Projections;
using Core.Queries;

namespace Carts.Carts.Queries
{
    public class GetCartById : IQuery<CartDetails>
    {
        public Guid CartId { get; }

        private GetCartById(Guid cartId)
        {
            CartId = cartId;
        }

        public static GetCartById Create(Guid cartId)
        {
            Guard.Against.Default(cartId, nameof(cartId));

            return new GetCartById(cartId);
        }
    }
}
