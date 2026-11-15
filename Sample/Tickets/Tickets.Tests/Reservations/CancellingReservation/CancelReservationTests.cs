using Shouldly;
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
        reservation.Status.ShouldBe(ReservationStatus.Cancelled);

        var @event = reservation.PublishedEvent<ReservationCancelled>();

        @event.ShouldNotBeNull();
        @event.ShouldBeOfType<ReservationCancelled>();
        @event!.ReservationId.ShouldBe(reservation.Id);
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
