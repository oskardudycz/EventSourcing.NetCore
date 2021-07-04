using FluentAssertions;
using Tickets.Reservations;
using Tickets.Reservations.ConfirmingReservation;
using Tickets.Tests.Builders;
using Tickets.Tests.Extensions;
using Xunit;

namespace Tickets.Tests.Reservations.ConfirmingReservation
{
    public class ConfirmReservationTests
    {
        [Fact]
        public void ForTentativeReservation_ShouldSucceed()
        {
            // Given
            var reservation = ReservationBuilder
                .Create()
                .Tentative()
                .Build();

            // When
            reservation.Confirm();

            // Then
            reservation.Status.Should().Be(ReservationStatus.Confirmed);
            reservation.Version.Should().Be(2);

            var @event = reservation.PublishedEvent<ReservationConfirmed>();

            @event.Should().NotBeNull();
            @event.Should().BeOfType<ReservationConfirmed>();
            @event!.ReservationId.Should().Be(reservation.Id);
        }
    }
}
