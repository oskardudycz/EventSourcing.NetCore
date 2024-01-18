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

internal class HandleChangeReservationSeat:
    ICommandHandler<ChangeReservationSeat>
{
    private readonly IMartenRepository<Reservation> repository;

    public HandleChangeReservationSeat(IMartenRepository<Reservation> repository) =>
        this.repository = repository;

    public Task Handle(ChangeReservationSeat command, CancellationToken ct) =>
        repository.GetAndUpdate(
            command.ReservationId,
            reservation => reservation.ChangeSeat(command.SeatId),
            ct: ct
        );
}
