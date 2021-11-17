using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Exceptions;
using Core.Queries;
using Marten;

namespace Tickets.Reservations.GettingReservationById;

public record GetReservationById(
    Guid ReservationId
): IQuery<ReservationDetails>;


internal class HandleGetReservationById :
    IQueryHandler<GetReservationById, ReservationDetails>
{
    private readonly IDocumentSession querySession;

    public HandleGetReservationById(IDocumentSession querySession)
    {
        this.querySession = querySession;
    }

    public async Task<ReservationDetails> Handle(GetReservationById request, CancellationToken cancellationToken)
    {
        return await querySession.LoadAsync<ReservationDetails>(request.ReservationId, cancellationToken)
               ?? throw AggregateNotFoundException.For<ReservationDetails>(request.ReservationId);
    }
}