using System;
using Ardalis.GuardClauses;
using Core.Queries;
using Tickets.Reservations.Projections;

namespace Tickets.Reservations.Queries
{
    public class GetReservationAtVersion : IQuery<ReservationDetails>
    {
        public Guid ReservationId { get; }
        public int Version { get; }

        private GetReservationAtVersion(Guid reservationId, int version)
        {
            ReservationId = reservationId;
            Version = version;
        }

        public static GetReservationAtVersion Create(Guid reservationId, int version)
        {
            Guard.Against.Default(reservationId, nameof(reservationId));
            Guard.Against.Negative(version, nameof(version));

            return new GetReservationAtVersion(reservationId, version);
        }
    }
}
