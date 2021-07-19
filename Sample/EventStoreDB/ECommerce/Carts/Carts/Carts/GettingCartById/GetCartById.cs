using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Exceptions;
using Core.Queries;
using Marten;

namespace Carts.Carts.GettingCartById
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

    internal class HandleGetCartById :
        IQueryHandler<GetCartById, CartDetails>
    {
        private readonly IDocumentSession querySession;

        public HandleGetCartById(IDocumentSession querySession)
        {
            this.querySession = querySession;
        }

        public async Task<CartDetails> Handle(GetCartById request, CancellationToken cancellationToken)
        {
            var cart = await querySession.LoadAsync<CartDetails>(request.CartId, cancellationToken);

            return cart ?? throw AggregateNotFoundException.For<Cart>(request.CartId);
        }
    }
}
