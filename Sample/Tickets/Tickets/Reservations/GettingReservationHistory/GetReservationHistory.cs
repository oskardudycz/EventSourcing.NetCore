using Core.Queries;
using Marten;
using Marten.Pagination;

namespace Tickets.Reservations.GettingReservationHistory;

public record GetReservationHistory(
    Guid ReservationId,
    int PageNumber,
    int PageSize
)
{
    public static GetReservationHistory Create(Guid reservationId, int pageNumber = 1, int pageSize = 20)
    {
        if (pageNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        if (pageSize is <= 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        return new GetReservationHistory(reservationId, pageNumber, pageSize);
    }
}

internal class HandleGetReservationHistory(IDocumentSession querySession):
    IQueryHandler<GetReservationHistory, IPagedList<ReservationHistory>>
{
    public Task<IPagedList<ReservationHistory>> Handle(
        GetReservationHistory query,
        CancellationToken cancellationToken
    )
    {
        var (reservationId, pageNumber, pageSize) = query;

        return querySession.Query<ReservationHistory>()
            .Where(h => h.ReservationId == reservationId)
            .ToPagedListAsync(pageNumber, pageSize, cancellationToken);
    }
}
