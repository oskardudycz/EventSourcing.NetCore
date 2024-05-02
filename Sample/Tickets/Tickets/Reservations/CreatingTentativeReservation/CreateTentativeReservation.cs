using Core.Commands;
using Core.Marten.Repository;
using Tickets.Reservations.NumberGeneration;

namespace Tickets.Reservations.CreatingTentativeReservation;

public record CreateTentativeReservation(
    Guid ReservationId,
    Guid SeatId
)
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

internal class HandleCreateTentativeReservation(
    IMartenRepository<Reservation> repository,
    IReservationNumberGenerator reservationNumberGenerator)
    :
        ICommandHandler<CreateTentativeReservation>
{
    public Task Handle(CreateTentativeReservation command, CancellationToken ct)
    {
        var (reservationId, seatId) = command;

        return repository.Add(
            Reservation.CreateTentative(
                reservationId,
                reservationNumberGenerator,
                seatId
            ),
            ct
        );
    }
}
