using Core.Queries;
using Marten;
using Marten.Pagination;

namespace Carts.ShoppingCarts.GettingCartHistory;

public record GetCartHistory(
    Guid CartId,
    int PageNumber,
    int PageSize
)
{
    public static GetCartHistory Create(Guid? cartId, int? pageNumber = 1, int? pageSize = 20)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (pageNumber is null or <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        if (pageSize is null or <= 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        return new GetCartHistory(cartId.Value, pageNumber.Value, pageSize.Value);
    }
}

internal class HandleGetCartHistory:
    IQueryHandler<GetCartHistory, IPagedList<CartHistory>>
{
    private readonly IDocumentSession querySession;

    public HandleGetCartHistory(IDocumentSession querySession)
    {
        this.querySession = querySession;
    }

    public Task<IPagedList<CartHistory>> Handle(GetCartHistory query, CancellationToken cancellationToken)
    {
        var (cartId, pageNumber, pageSize) = query;

        return querySession.Query<CartHistory>()
            .Where(h => h.CartId == cartId)
            .ToPagedListAsync(pageNumber, pageSize, cancellationToken);
    }
}
