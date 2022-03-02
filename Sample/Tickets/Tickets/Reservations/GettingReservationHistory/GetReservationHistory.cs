using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Queries;
using Marten;
using Marten.Pagination;

namespace Tickets.Reservations.GettingReservationHistory;

public record GetReservationHistory(
    Guid ReservationId,
    int PageNumber,
    int PageSize
): IQuery<IPagedList<ReservationHistory>>
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

internal class HandleGetReservationHistory:
    IQueryHandler<GetReservationHistory, IPagedList<ReservationHistory>>
{
    private readonly IDocumentSession querySession;

    public HandleGetReservationHistory(IDocumentSession querySession)
    {
        this.querySession = querySession;
    }

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
