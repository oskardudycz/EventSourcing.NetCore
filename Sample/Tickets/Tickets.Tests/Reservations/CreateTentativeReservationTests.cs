using System;
using FluentAssertions;
using Tickets.Reservations;
using Tickets.Reservations.Events;
using Tickets.Tests.Extensions;
using Tickets.Tests.Stubs.Ids;
using Xunit;

namespace Tickets.Tests.Reservations
{
    public class CreateTentativeReservation
    {
        [Fact]
        public void ForValidParams_ShouldCreateReservationWithTentativeStatus()
        {
            // Given
            var idGenerator = new FakeAggregateIdGenerator<Reservation>();
            var seatId = Guid.NewGuid();
            const string number = "abcd/123";

            // When
            var reservation = Reservation.CreateTentative(
                idGenerator,
                seatId,
                number
            );

            // Then
            idGenerator.LastGeneratedId.Should().NotBeNull();

            reservation.Status.Should().Be(ReservationStatus.Tentative);

            reservation.Id.Should().Be(idGenerator.LastGeneratedId.Value);
            reservation.Number.Should().Be(number);
            reservation.SeatId.Should().Be(seatId);
            reservation.Version.Should().Be(1);

            var @event = reservation.PublishedEvent<TentativeReservationCreated>();

            @event.Should().BeOfType<TentativeReservationCreated>();
            @event.ReservationId.Should().Be(reservation.Id);
            @event.Number.Should().Be(reservation.Number);
            @event.SeatId.Should().Be(reservation.SeatId);
        }
    }
}
