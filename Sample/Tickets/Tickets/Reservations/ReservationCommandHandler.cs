using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Commands;
using Core.Ids;
using Core.Storage;
using MediatR;
using Tickets.Reservations.Commands;

namespace Tickets.Reservations
{
    internal class ReservationCommandHandler: ICommandHandler<CreateTentativeReservation>
    {
        private readonly IRepository<Reservation> repository;
        private readonly IAggregateIdGenerator<Reservation> aggregateIdGenerator;
        private readonly IReservationNumberGenerator reservationNumberGenerator;

        public ReservationCommandHandler(
            IRepository<Reservation> repository,
            IAggregateIdGenerator<Reservation> aggregateIdGenerator,
            IReservationNumberGenerator reservationNumberGenerator
        )
        {
            Guard.Against.Null(repository, nameof(repository));
            Guard.Against.Null(aggregateIdGenerator, nameof(aggregateIdGenerator));
            Guard.Against.Null(reservationNumberGenerator, nameof(reservationNumberGenerator));

            this.repository = repository;
            this.aggregateIdGenerator = aggregateIdGenerator;
            this.reservationNumberGenerator = reservationNumberGenerator;
        }

        public Task<Unit> Handle(CreateTentativeReservation command, CancellationToken cancellationToken)
        {
            Guard.Against.Null(repository, nameof(repository));

            var reservation = Reservation.CreateTentative(
                aggregateIdGenerator,
                reservationNumberGenerator,
                command.SeatId
            );

            repository.Add(reservation, cancellationToken);

            return Unit.Task;
        }
    }
}
