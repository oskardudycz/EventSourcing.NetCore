using Core.Commands;
using Core.Marten.Repository;

namespace Tickets.Reservations.CancellingReservation;

public record CancelReservation(
    Guid ReservationId
)
{
    public static CancelReservation Create(Guid? reservationId)
    {
        if (!reservationId.HasValue || reservationId == Guid.Empty)
            throw new ArgumentNullException(nameof(reservationId));

        return new CancelReservation(reservationId.Value);
    }
}

internal class HandleCancelReservation(IMartenRepository<Reservation> repository):
    ICommandHandler<CancelReservation>
{
    public Task Handle(CancelReservation command, CancellationToken ct) =>
        repository.GetAndUpdate(
            command.ReservationId,
            reservation => reservation.Cancel(),
            ct: ct
        );
}
