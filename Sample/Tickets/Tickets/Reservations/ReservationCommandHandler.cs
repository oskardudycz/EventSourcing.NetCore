using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Commands;
using Core.Storage;
using MediatR;
using Tickets.Reservations.Commands;
using Tickets.Reservations.Events;

namespace Tickets.Reservations
{
    internal class ReservationCommandHandler:
        ICommandHandler<CreateTentativeReservation>,
        ICommandHandler<ChangeReservationSeat>,
        ICommandHandler<ConfirmReservation>,
        ICommandHandler<CancelReservation>
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
            Guard.Against.Null(command, nameof(command));

            var reservation = Reservation.CreateTentative(
                command.ReservationId,
                reservationNumberGenerator,
                command.SeatId
            );

            await repository.Add(reservation, cancellationToken);

            return Unit.Value;
        }

        public async Task<Unit> Handle(ChangeReservationSeat command, CancellationToken cancellationToken)
        {
            Guard.Against.Null(command, nameof(command));

            var reservation = await repository.Find(command.ReservationId, cancellationToken);

            reservation.ChangeSeat(command.SeatId);

            await repository.Update(reservation, cancellationToken);

            return Unit.Value;
        }

        public async Task<Unit> Handle(ConfirmReservation command, CancellationToken cancellationToken)
        {
            Guard.Against.Null(command, nameof(command));

            var reservation = await repository.Find(command.ReservationId, cancellationToken);

            reservation.Confirm();

            await repository.Update(reservation, cancellationToken);

            return Unit.Value;
        }

        public async Task<Unit> Handle(CancelReservation command, CancellationToken cancellationToken)
        {
            Guard.Against.Null(command, nameof(command));

            var reservation = await repository.Find(command.ReservationId, cancellationToken);

            reservation.Cancel();

            await repository.Update(reservation, cancellationToken);

            return Unit.Value;
        }
    }
}
