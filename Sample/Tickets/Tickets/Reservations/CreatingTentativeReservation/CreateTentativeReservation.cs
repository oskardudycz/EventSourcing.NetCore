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

internal class HandleCreateTentativeReservation:
    ICommandHandler<CreateTentativeReservation>
{
    private readonly IMartenRepository<Reservation> repository;
    private readonly IReservationNumberGenerator reservationNumberGenerator;

    public HandleCreateTentativeReservation(
        IMartenRepository<Reservation> repository,
        IReservationNumberGenerator reservationNumberGenerator
    )
    {
        this.repository = repository;
        this.reservationNumberGenerator = reservationNumberGenerator;
    }

    public Task Handle(CreateTentativeReservation command, CancellationToken cancellationToken)
    {
        var (reservationId, seatId) = command;

        return repository.Add(
            Reservation.CreateTentative(
                reservationId,
                reservationNumberGenerator,
                seatId
            ),
            cancellationToken
        );
    }
}
