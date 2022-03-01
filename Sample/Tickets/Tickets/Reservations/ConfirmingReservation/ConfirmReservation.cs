using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Exceptions;
using Core.Marten.Repository;
using MediatR;

namespace Tickets.Reservations.ConfirmingReservation;

public class ConfirmReservation : ICommand
{
    public Guid ReservationId { get; }

    private ConfirmReservation(Guid reservationId)
    {
        ReservationId = reservationId;
    }

    public static ConfirmReservation Create(Guid? reservationId)
    {
        if (!reservationId.HasValue)
            throw new ArgumentNullException(nameof(reservationId));

        return new ConfirmReservation(reservationId.Value);
    }
}

internal class HandleConfirmReservation:
    ICommandHandler<ConfirmReservation>
{
    private readonly IMartenRepository<Reservation> repository;

    public HandleConfirmReservation(
        IMartenRepository<Reservation> repository
    )
    {
        this.repository = repository;
    }

    public async Task<Unit> Handle(ConfirmReservation command, CancellationToken cancellationToken)
    {
        var reservation = await repository.Find(command.ReservationId, cancellationToken)
                          ?? throw AggregateNotFoundException.For<Reservation>(command.ReservationId);

        reservation.Confirm();

        await repository.Update(reservation, cancellationToken);

        return Unit.Value;
    }
}
