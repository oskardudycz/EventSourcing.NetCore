using Core.Commands;
using Core.Marten.Events;
using Core.Marten.Repository;
using MediatR;

namespace Tickets.Reservations.ChangingReservationSeat;

public record ChangeReservationSeat(
    Guid ReservationId,
    Guid SeatId
): ICommand
{
    public static ChangeReservationSeat Create(Guid? reservationId, Guid? seatId)
    {
        if (!reservationId.HasValue)
            throw new ArgumentNullException(nameof(reservationId));
        if (!seatId.HasValue)
            throw new ArgumentNullException(nameof(seatId));

        return new ChangeReservationSeat(
            reservationId.Value,
            seatId.Value
        );
    }
}

internal class HandleChangeReservationSeat:
    ICommandHandler<ChangeReservationSeat>
{
    private readonly IMartenRepository<Reservation> repository;
    private readonly IMartenAppendScope scope;

    public HandleChangeReservationSeat(
        IMartenRepository<Reservation> repository,
        IMartenAppendScope scope
    )
    {
        this.repository = repository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(ChangeReservationSeat command, CancellationToken cancellationToken)
    {
        await scope.Do((expectedVersion, traceMetadata) =>
            repository.GetAndUpdate(
                command.ReservationId,
                reservation => reservation.ChangeSeat(command.SeatId),
                expectedVersion,
                traceMetadata,
                cancellationToken
            )
        );
        return Unit.Value;
    }
}
