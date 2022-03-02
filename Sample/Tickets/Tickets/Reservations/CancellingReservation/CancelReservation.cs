using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Marten.OptimisticConcurrency;
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
    private readonly MartenOptimisticConcurrencyScope scope;

    public HandleCancelReservation(
        IMartenRepository<Reservation> repository,
        MartenOptimisticConcurrencyScope scope
    )
    {
        this.repository = repository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(CancelReservation command, CancellationToken cancellationToken)
    {
        await scope.Do(expectedVersion =>
            repository.GetAndUpdate(
                command.ReservationId,
                reservation => reservation.Cancel(),
                expectedVersion,
                cancellationToken
            )
        );
        return Unit.Value;
    }
}
