using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Commands;
using Core.Storage;
using MediatR;
using Tickets.Reservations.Commands;

namespace Tickets.Reservations
{
    internal class ReservationCommandHandler: ICommandHandler<CreateTentativeReservation>
    {
        private readonly IRepository<Reservation> repository;
        private readonly IReservationNumberGenerator reservationNumberGenerator;

        public ReservationCommandHandler(
            IRepository<Reservation> repository,
            IReservationNumberGenerator reservationNumberGenerator
        )
        {
            Guard.Against.Null(repository, nameof(repository));
            Guard.Against.Null(reservationNumberGenerator, nameof(reservationNumberGenerator));

            this.repository = repository;
            this.reservationNumberGenerator = reservationNumberGenerator;
        }

        public async Task<Unit> Handle(CreateTentativeReservation command, CancellationToken cancellationToken)
        {
            Guard.Against.Null(repository, nameof(repository));

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
