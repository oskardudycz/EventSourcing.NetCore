using Core.Commands;
using Core.Marten.Repository;

namespace Tickets.Reservations.ChangingReservationSeat;

public record ChangeReservationSeat(
    Guid ReservationId,
    Guid SeatId
)
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

internal class HandleChangeReservationSeat(IMartenRepository<Reservation> repository):
    ICommandHandler<ChangeReservationSeat>
{
    public Task Handle(ChangeReservationSeat command, CancellationToken ct) =>
        repository.GetAndUpdate(
            command.ReservationId,
            reservation => reservation.ChangeSeat(command.SeatId),
            ct: ct
        );
}
