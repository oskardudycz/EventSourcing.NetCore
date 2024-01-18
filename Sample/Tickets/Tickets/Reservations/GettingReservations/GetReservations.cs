using Core.Queries;
using Marten;
using Marten.Pagination;

namespace Tickets.Reservations.GettingReservations;

public record GetReservations(
    int PageNumber,
    int PageSize
)
{
    public static GetReservations Create(int pageNumber = 1, int pageSize = 20)
    {
        if (pageNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        if (pageSize is <= 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        return new GetReservations(pageNumber, pageSize);
    }
}

internal class HandleGetReservations:
    IQueryHandler<GetReservations, IPagedList<ReservationShortInfo>>
{
    private readonly IDocumentSession querySession;

    public HandleGetReservations(IDocumentSession querySession)
    {
        this.querySession = querySession;
    }

    public Task<IPagedList<ReservationShortInfo>> Handle(
        GetReservations query,
        CancellationToken cancellationToken
    ) =>
        querySession.Query<ReservationShortInfo>()
            .ToPagedListAsync(query.PageNumber, query.PageSize, cancellationToken);
}
