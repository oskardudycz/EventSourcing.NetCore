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

internal class HandleConfirmReservation:
    ICommandHandler<ConfirmReservation>
{
    private readonly IMartenRepository<Reservation> repository;

    public HandleConfirmReservation(IMartenRepository<Reservation> repository) =>
        this.repository = repository;

    public Task Handle(ConfirmReservation command, CancellationToken cancellationToken) =>
        repository.GetAndUpdate(
            command.ReservationId,
            payment => payment.Confirm(),
            cancellationToken: cancellationToken
        );
}
