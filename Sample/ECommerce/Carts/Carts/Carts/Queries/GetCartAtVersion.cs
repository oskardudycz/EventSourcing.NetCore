using System;
using Ardalis.GuardClauses;
using Carts.Carts.Projections;
using Core.Queries;

namespace Carts.Carts.Queries
{
    public class GetCartAtVersion : IQuery<CartDetails>
    {
        public Guid CartId { get; }
        public int Version { get; }

        private GetCartAtVersion(Guid cartId, int version)
        {
            CartId = cartId;
            Version = version;
        }

        public static GetCartAtVersion Create(Guid cartId, int version)
        {
            Guard.Against.Default(cartId, nameof(cartId));
            Guard.Against.Negative(version, nameof(version));

            return new GetCartAtVersion(cartId, version);
        }
    }
}
