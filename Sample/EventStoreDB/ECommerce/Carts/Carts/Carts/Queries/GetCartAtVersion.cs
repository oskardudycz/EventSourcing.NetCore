using System;
using Ardalis.GuardClauses;
using Carts.Carts.Projections;
using Core.Queries;

namespace Carts.Carts.Queries
{
    public class GetCartAtVersion : IQuery<CartDetails>
    {
        public Guid CartId { get; }
        public ulong Version { get; }

        private GetCartAtVersion(Guid cartId, ulong version)
        {
            CartId = cartId;
            Version = version;
        }

        public static GetCartAtVersion Create(Guid cartId, ulong version)
        {
            Guard.Against.Default(cartId, nameof(cartId));

            return new GetCartAtVersion(cartId, version);
        }
    }
}
