using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Repositories;
using MediatR;
using Tickets.Reservations.NumberGeneration;

namespace Tickets.Reservations.CreatingTentativeReservation
{
    public class CreateTentativeReservation : ICommand
    {
        public Guid ReservationId { get; }
        public Guid SeatId { get; }

        private CreateTentativeReservation(Guid reservationId, Guid seatId)
        {
            ReservationId = reservationId;
            SeatId = seatId;
        }

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
        private readonly IRepository<Reservation> repository;
        private readonly IReservationNumberGenerator reservationNumberGenerator;

        public HandleCreateTentativeReservation(
            IRepository<Reservation> repository,
            IReservationNumberGenerator reservationNumberGenerator
        )
        {
            this.repository = repository;
            this.reservationNumberGenerator = reservationNumberGenerator;
        }

        public async Task<Unit> Handle(CreateTentativeReservation command, CancellationToken cancellationToken)
        {
            var reservation = Reservation.CreateTentative(
                command.ReservationId,
                reservationNumberGenerator,
                command.SeatId
            );

            await repository.Add(reservation, cancellationToken);

            return Unit.Value;
        }
    }
}
