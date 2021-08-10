using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Exceptions;
using Core.Repositories;
using MediatR;

namespace Tickets.Reservations.ChangingReservationSeat
{
    public class ChangeReservationSeat : ICommand
    {
        public Guid ReservationId { get; }
        public Guid SeatId { get; }

        private ChangeReservationSeat(Guid reservationId, Guid seatId)
        {
            ReservationId = reservationId;
            SeatId = seatId;
        }

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
        private readonly IRepository<Reservation> repository;

        public HandleChangeReservationSeat(
            IRepository<Reservation> repository
        )
        {
            this.repository = repository;
        }

        public async Task<Unit> Handle(ChangeReservationSeat command, CancellationToken cancellationToken)
        {
            var reservation = await repository.Find(command.ReservationId, cancellationToken)
                              ?? throw AggregateNotFoundException.For<Reservation>(command.ReservationId);

            reservation.ChangeSeat(command.SeatId);

            await repository.Update(reservation, cancellationToken);

            return Unit.Value;
        }
    }
}
