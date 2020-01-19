using System;
using Core.Aggregates;
using Tickets.Reservations;
using Tickets.Tests.Stubs.Ids;

namespace Tickets.Tests.Builders
{
    internal class ReservationBuilder
    {
        private Func<Reservation> build  = () => null;

        public ReservationBuilder Tentative()
        {
            var idGenerator = new FakeAggregateIdGenerator<Reservation>();
            var seatId = Guid.NewGuid();
            const string number = "abcd/123";

            // When
            var reservation = Reservation.CreateTentative(
                idGenerator,
                seatId,
                number
            );

            build = () => reservation;

            return this;
        }

        public static ReservationBuilder Create() => new ReservationBuilder();

        public Reservation Build()
        {
            var reservation = build();
            ((IAggregate)reservation).DequeueUncommittedEvents();
            return reservation;
        }
    }
}
