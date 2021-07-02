using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Carts.Carts.Projections;
using Carts.Carts.Queries;
using Core.Exceptions;
using Core.Queries;
using Marten;
using Marten.Pagination;
using MediatR;

namespace Carts.Carts
{
    internal class CartQueryHandler :
        IQueryHandler<GetCartById, CartDetails?>,
        IRequestHandler<GetCarts, IPagedList<CartShortInfo>>,
        IRequestHandler<GetCartHistory, IPagedList<CartHistory>>,
        IRequestHandler<GetCartAtVersion, CartDetails>
    {
        private readonly IDocumentSession querySession;

        public CartQueryHandler(IDocumentSession querySession)
        {
            Guard.Against.Null(querySession, nameof(querySession));

            this.querySession = querySession;
        }

        public Task<CartDetails?> Handle(GetCartById request, CancellationToken cancellationToken)
        {
            return querySession.LoadAsync<CartDetails>(request.CartId, cancellationToken);
        }

        public Task<IPagedList<CartShortInfo>> Handle(GetCarts request, CancellationToken cancellationToken)
        {
            return querySession.Query<CartShortInfo>()
                .ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken);
        }

        public Task<IPagedList<CartHistory>> Handle(GetCartHistory request, CancellationToken cancellationToken)
        {
            return querySession.Query<CartHistory>()
                .Where(h => h.CartId == request.CartId)
                .ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken);
        }

        public Task<CartDetails> Handle(GetCartAtVersion request, CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }
}
