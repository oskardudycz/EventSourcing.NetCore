using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Marten.OptimisticConcurrency;
using Core.Marten.Repository;
using MediatR;

namespace Tickets.Reservations.ChangingReservationSeat;

public record ChangeReservationSeat(
    Guid ReservationId,
    Guid SeatId
): ICommand
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
    private readonly MartenOptimisticConcurrencyScope scope;

    public HandleChangeReservationSeat(
        IMartenRepository<Reservation> repository,
        MartenOptimisticConcurrencyScope scope
    )
    {
        this.repository = repository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(ChangeReservationSeat command, CancellationToken cancellationToken)
    {
        await scope.Do(expectedVersion =>
            repository.GetAndUpdate(
                command.ReservationId,
                reservation => reservation.ChangeSeat(command.SeatId),
                expectedVersion,
                cancellationToken
            )
        );
        return Unit.Value;
    }
}
