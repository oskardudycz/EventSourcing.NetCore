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

internal class HandleCancelReservation:
    ICommandHandler<CancelReservation>
{
    private readonly IMartenRepository<Reservation> repository;

    public HandleCancelReservation(IMartenRepository<Reservation> repository) =>
        this.repository = repository;

    public Task Handle(CancelReservation command, CancellationToken ct) =>
        repository.GetAndUpdate(
            command.ReservationId,
            reservation => reservation.Cancel(),
            ct: ct
        );
}
