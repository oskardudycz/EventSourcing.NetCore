using System;
using Core.Commands;

namespace Tickets.Reservations.Commands
{
    public class CreateTentativeReservation : ICommand
    {
        public Guid SeatId { get; }

        private CreateTentativeReservation(Guid seatId)
        {
            SeatId = seatId;
        }

        public static CreateTentativeReservation Create(Guid seatId)
        {
            return new CreateTentativeReservation(seatId);
        }
    }
}
