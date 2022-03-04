using Core.Commands;
using Core.Marten.Events;
using Core.Marten.Repository;
using MediatR;
using Tickets.Reservations.NumberGeneration;

namespace Tickets.Reservations.CreatingTentativeReservation;

public record CreateTentativeReservation(
    Guid ReservationId,
    Guid SeatId
    ) : ICommand
{
    public static CreateTentativeReservation Create(Guid? reservationId, Guid? seatId)
    {
        if (!reservationId.HasValue)
            throw new ArgumentNullException(nameof(reservationId));
        if (!seatId.HasValue)
            throw new ArgumentNullException(nameof(seatId));

        return new CreateTentativeReservation(reservationId.Value, seatId.Value);
    }
}

internal class HandleCreateTentativeReservation:
    ICommandHandler<CreateTentativeReservation>
{
    private readonly IMartenRepository<Reservation> repository;
    private readonly IReservationNumberGenerator reservationNumberGenerator;
    private readonly IMartenAppendScope scope;

    public HandleCreateTentativeReservation(
        IMartenRepository<Reservation> repository,
        IReservationNumberGenerator reservationNumberGenerator,
        IMartenAppendScope scope
    )
    {
        this.repository = repository;
        this.reservationNumberGenerator = reservationNumberGenerator;
        this.scope = scope;
    }

    public async Task<Unit> Handle(CreateTentativeReservation command, CancellationToken cancellationToken)
    {
        var (reservationId, seatId) = command;

        await scope.Do((_, eventMetadata) =>
            repository.Add(
                Reservation.CreateTentative(
                    reservationId,
                    reservationNumberGenerator,
                    seatId
                ),
                eventMetadata,
                cancellationToken
            )
        );
        return Unit.Value;
    }
}
