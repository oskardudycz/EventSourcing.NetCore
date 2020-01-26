using System;
using Ardalis.GuardClauses;
using Core.Queries;
using Tickets.Reservations.Projections;

namespace Tickets.Reservations.Queries
{
    public class GetReservationById : IQuery<ReservationDetails>
    {
        public Guid ReservationId { get; }

        private GetReservationById(Guid reservationId)
        {
            ReservationId = reservationId;
        }

        public static GetReservationById Create(Guid reservationId)
        {
            Guard.Against.Default(reservationId, nameof(reservationId));

            return new GetReservationById(reservationId);
        }
    }
}
