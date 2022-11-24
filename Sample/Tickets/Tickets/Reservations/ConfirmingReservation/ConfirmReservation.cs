using Core.Commands;
using Core.Marten.Events;
using Core.Marten.Repository;
using MediatR;

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
    private readonly IMartenAppendScope scope;

    public HandleConfirmReservation(
        IMartenRepository<Reservation> repository,
        IMartenAppendScope scope
    )
    {
        this.repository = repository;
        this.scope = scope;
    }

    public async Task Handle(ConfirmReservation command, CancellationToken cancellationToken)
    {
        await scope.Do(expectedVersion =>
            repository.GetAndUpdate(
                command.ReservationId,
                payment => payment.Confirm(),
                expectedVersion,
                cancellationToken
            )
        );
    }
}
