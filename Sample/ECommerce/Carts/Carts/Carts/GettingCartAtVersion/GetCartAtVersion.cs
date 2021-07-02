using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Carts.Carts.GettingCartById;
using Core.Exceptions;
using Core.Queries;
using Marten;

namespace Carts.Carts.GettingCartAtVersion
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

    internal class HandleGetCartAtVersion :
        IQueryHandler<GetCartAtVersion, CartDetails>
    {
        private readonly IDocumentSession querySession;

        public HandleGetCartAtVersion(IDocumentSession querySession)
        {
            this.querySession = querySession;
        }

        public async Task<CartDetails> Handle(GetCartAtVersion request, CancellationToken cancellationToken)
        {
            return await querySession.Events.AggregateStreamAsync<CartDetails>(request.CartId, request.Version, token: cancellationToken)
                   ?? throw AggregateNotFoundException.For<Cart>(request.CartId);
        }
    }
}
