using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Exceptions;
using Core.Marten.Repository;
using MediatR;

namespace Tickets.Reservations.CancellingReservation;

public class CancelReservation : ICommand
{
    public Guid ReservationId { get; }

    private CancelReservation(Guid reservationId)
    {
        ReservationId = reservationId;
    }

    public static CancelReservation Create(Guid? reservationId)
    {
        if (!reservationId.HasValue)
            throw new ArgumentNullException(nameof(reservationId));

        return new CancelReservation(reservationId.Value);
    }
}

internal class HandleCancelReservation:
    ICommandHandler<CancelReservation>
{
    private readonly IMartenRepository<Reservation> repository;

    public HandleCancelReservation(
        IMartenRepository<Reservation> repository
    )
    {
        this.repository = repository;
    }

    public async Task<Unit> Handle(CancelReservation command, CancellationToken cancellationToken)
    {
        var reservation = await repository.Find(command.ReservationId, cancellationToken)
                          ?? throw AggregateNotFoundException.For<Reservation>(command.ReservationId);

        reservation.Cancel();

        await repository.Update(reservation, cancellationToken);

        return Unit.Value;
    }
}
