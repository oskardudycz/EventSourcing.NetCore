using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Carts.Carts.GettingCartHistory;
using Core.Queries;
using Marten;
using Marten.Pagination;

namespace Carts.Carts.Queries
{
    public class GetCartHistory : IQuery<IPagedList<CartHistory>>
    {
        public Guid CartId { get; }
        public int PageNumber { get; }
        public int PageSize { get; }

        private GetCartHistory(Guid cartId, int pageNumber, int pageSize)
        {
            CartId = cartId;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public static GetCartHistory Create(Guid cartId,int pageNumber = 1, int pageSize = 20)
        {
            if (pageNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageNumber));
            if (pageSize is <= 0 or > 100)
                throw new ArgumentOutOfRangeException(nameof(pageSize));

            return new GetCartHistory(cartId, pageNumber, pageSize);
        }
    }

    internal class HandleGetCartHistory :
        IQueryHandler<GetCartHistory, IPagedList<CartHistory>>
    {
        private readonly IDocumentSession querySession;

        public HandleGetCartHistory(IDocumentSession querySession)
        {
            this.querySession = querySession;
        }

        public Task<IPagedList<CartHistory>> Handle(GetCartHistory request, CancellationToken cancellationToken)
        {
            return querySession.Query<CartHistory>()
                .Where(h => h.CartId == request.CartId)
                .ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken);
        }
    }
}
