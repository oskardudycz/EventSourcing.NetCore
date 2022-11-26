using Core.Exceptions;
using Core.Queries;
using Marten;
using Tickets.Reservations.GettingReservationById;

namespace Tickets.Reservations.GettingReservationAtVersion;

public record GetReservationAtVersion(
    Guid ReservationId,
    int Version
)
{
    public static GetReservationAtVersion Create(Guid reservationId, int version)
    {
        if (reservationId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(reservationId));
        if (version < 0)
            throw new ArgumentOutOfRangeException(nameof(version));

        return new GetReservationAtVersion(reservationId, version);
    }
}

internal class HandleGetReservationAtVersion:
    IQueryHandler<GetReservationAtVersion, ReservationDetails>
{
    private readonly IDocumentSession querySession;

    public HandleGetReservationAtVersion(IDocumentSession querySession)
    {
        this.querySession = querySession;
    }

    public async Task<ReservationDetails> Handle(GetReservationAtVersion query, CancellationToken cancellationToken)
    {
        var (reservationId, version) = query;
        return await querySession.Events.AggregateStreamAsync<ReservationDetails>(
            reservationId,
            version,
            token: cancellationToken
        ) ?? throw AggregateNotFoundException.For<ReservationDetails>(reservationId);
    }
}
