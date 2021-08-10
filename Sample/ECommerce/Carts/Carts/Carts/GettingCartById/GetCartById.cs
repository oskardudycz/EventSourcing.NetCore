using System;
using System.Threading;
using System.Threading.Tasks;
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
            if (cartId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(cartId));

            return new GetCartById(cartId);
        }
    }

    internal class HandleGetCartById :
        IQueryHandler<GetCartById, CartDetails?>
    {
        private readonly IDocumentSession querySession;

        public HandleGetCartById(IDocumentSession querySession)
        {
            this.querySession = querySession;
        }

        public Task<CartDetails?> Handle(GetCartById request, CancellationToken cancellationToken)
        {
            return querySession.LoadAsync<CartDetails>(request.CartId, cancellationToken);
        }
    }
}
