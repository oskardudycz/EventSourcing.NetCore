using Core.Commands;
using Core.Marten.Repository;

namespace Tickets.Reservations.ConfirmingReservation;

public record ConfirmReservation(
    Guid ReservationId
)
{
    public static ConfirmReservation Create(Guid? reservationId)
    {
        if (!reservationId.HasValue)
            throw new ArgumentNullException(nameof(reservationId));

        return new ConfirmReservation(reservationId.Value);
    }
}

internal class HandleConfirmReservation(IMartenRepository<Reservation> repository):
    ICommandHandler<ConfirmReservation>
{
    public Task Handle(ConfirmReservation command, CancellationToken ct) =>
        repository.GetAndUpdate(
            command.ReservationId,
            payment => payment.Confirm(),
            ct: ct
        );
}
