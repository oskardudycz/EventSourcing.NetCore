using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Queries;
using Marten;
using Marten.Pagination;

namespace Carts.ShoppingCarts.GettingCarts;

public record GetCarts(
    int PageNumber,
    int PageSize
): IQuery<IPagedList<ShoppingCartShortInfo>>
{
    public static GetCarts Create(int? pageNumber = 1, int? pageSize = 20)
    {
        if (pageNumber is null or <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        if (pageSize is null or <= 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        return new GetCarts(pageNumber.Value, pageSize.Value);
    }
}

internal class HandleGetCarts:
    IQueryHandler<GetCarts, IPagedList<ShoppingCartShortInfo>>
{
    private readonly IDocumentSession querySession;

    public HandleGetCarts(IDocumentSession querySession)
    {
        this.querySession = querySession;
    }

    public Task<IPagedList<ShoppingCartShortInfo>> Handle(GetCarts query, CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = query;

        return querySession.Query<ShoppingCartShortInfo>()
            .ToPagedListAsync(pageNumber, pageSize, cancellationToken);
    }
}
