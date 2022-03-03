using Core.Commands;
using Core.Marten.OptimisticConcurrency;
using Core.Marten.Repository;
using MediatR;

namespace Tickets.Reservations.ConfirmingReservation;

public record ConfirmReservation(
    Guid ReservationId
): ICommand
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
    private readonly MartenOptimisticConcurrencyScope scope;

    public HandleConfirmReservation(
        IMartenRepository<Reservation> repository,
        MartenOptimisticConcurrencyScope scope
    )
    {
        this.repository = repository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(ConfirmReservation command, CancellationToken cancellationToken)
    {
        await scope.Do(expectedVersion =>
            repository.GetAndUpdate(
                command.ReservationId,
                payment => payment.Confirm(),
                expectedVersion,
                cancellationToken)
        );

        return Unit.Value;
    }
}
