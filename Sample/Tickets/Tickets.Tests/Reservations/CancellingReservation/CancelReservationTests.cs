using FluentAssertions;
using Tickets.Reservations;
using Tickets.Reservations.CancellingReservation;
using Tickets.Tests.Builders;
using Tickets.Tests.Extensions;
using Xunit;

namespace Tickets.Tests.Reservations.CancellingReservation;

public class CancelReservationTests
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
        reservation.Cancel();

        // Then
        reservation.Status.Should().Be(ReservationStatus.Cancelled);
        reservation.Version.Should().Be(2);

        var @event = reservation.PublishedEvent<ReservationCancelled>();

        @event.Should().NotBeNull();
        @event.Should().BeOfType<ReservationCancelled>();
        @event!.ReservationId.Should().Be(reservation.Id);
    }

    [Fact]
    public void ForConfirmedReservation_ShouldFailWithInvalidOperation()
    {

    }

    [Fact]
    public void ForCancelledReservation_ShouldFailWithInvalidOperation()
    {

    }
}