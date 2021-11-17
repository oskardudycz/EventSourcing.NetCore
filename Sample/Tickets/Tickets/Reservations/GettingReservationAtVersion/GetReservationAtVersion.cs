using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Exceptions;
using Core.Queries;
using Marten;
using Tickets.Reservations.GettingReservationById;

namespace Tickets.Reservations.GettingReservationAtVersion;

public class GetReservationAtVersion : IQuery<ReservationDetails>
{
    public Guid ReservationId { get; }
    public int Version { get; }

    private GetReservationAtVersion(Guid reservationId, int version)
    {
        ReservationId = reservationId;
        Version = version;
    }

    public static GetReservationAtVersion Create(Guid reservationId, int version)
    {
        if (reservationId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(reservationId));
        if (version < 0)
            throw new ArgumentOutOfRangeException(nameof(version));

        return new GetReservationAtVersion(reservationId, version);
    }
}

internal class HandleGetReservationAtVersion :
    IQueryHandler<GetReservationAtVersion, ReservationDetails>
{
    private readonly IDocumentSession querySession;

    public HandleGetReservationAtVersion(IDocumentSession querySession)
    {
        this.querySession = querySession;
    }

    public async Task<ReservationDetails> Handle(GetReservationAtVersion request, CancellationToken cancellationToken)
    {
        return await querySession.Events.AggregateStreamAsync<ReservationDetails>(request.ReservationId, request.Version, token: cancellationToken)
               ?? throw AggregateNotFoundException.For<ReservationDetails>(request.ReservationId);
    }
}