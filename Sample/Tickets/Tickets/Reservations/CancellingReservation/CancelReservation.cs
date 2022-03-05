using Core.Commands;
using Core.Marten.Events;
using Core.Marten.Repository;
using MediatR;

namespace Tickets.Reservations.CancellingReservation;

public record CancelReservation(
    Guid ReservationId
) : ICommand
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
    private readonly IMartenAppendScope scope;

    public HandleCancelReservation(
        IMartenRepository<Reservation> repository,
        IMartenAppendScope scope
    )
    {
        this.repository = repository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(CancelReservation command, CancellationToken cancellationToken)
    {
        await scope.Do((expectedVersion, traceMetadata) =>
            repository.GetAndUpdate(
                command.ReservationId,
                reservation => reservation.Cancel(),
                expectedVersion,
                traceMetadata,
                cancellationToken
            )
        );
        return Unit.Value;
    }
}
