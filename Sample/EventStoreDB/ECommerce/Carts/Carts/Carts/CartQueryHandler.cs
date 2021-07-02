using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Carts.Carts.Projections;
using Carts.Carts.Queries;
using Core.EventStoreDB.Events;
using Core.Exceptions;
using Core.Queries;
using EventStore.Client;
using Marten;
using Marten.Pagination;
using MediatR;

namespace Carts.Carts
{
    internal class CartQueryHandler:
        IQueryHandler<GetCartById, CartDetails?>,
        IRequestHandler<GetCarts, IPagedList<CartShortInfo>>,
        IRequestHandler<GetCartHistory, IPagedList<CartHistory>>,
        IRequestHandler<GetCartAtVersion, CartDetails>
    {
        private readonly IDocumentSession querySession;
        private readonly EventStoreClient eventStore;

        public CartQueryHandler(
            IDocumentSession querySession,
            EventStoreClient eventStore
        )
        {
            Guard.Against.Null(querySession, nameof(querySession));
            Guard.Against.Null(eventStore, nameof(eventStore));

            this.querySession = querySession;
            this.eventStore = eventStore;
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

        public async Task<CartDetails> Handle(GetCartAtVersion request, CancellationToken cancellationToken)
        {
            var cart = await eventStore.AggregateStream<CartDetails>(
                request.CartId,
                cancellationToken,
                request.Version
            );

            if (cart == null)
                throw AggregateNotFoundException.For<Cart>(request.CartId);

            return cart;
        }
    }
}
