using Shouldly;
using Tickets.Reservations.ChangingReservationSeat;
using Tickets.Tests.Builders;
using Tickets.Tests.Extensions;
using Xunit;

namespace Tickets.Tests.Reservations.ChangingReservationSeat;

public class ChangeSeatTests
{
    [Fact]
    public void ForValidParams_UpdatesSeatId()
    {
        // Given
        var reservation = ReservationBuilder
            .Create()
            .Tentative()
            .Build();

        var newSeatId = Guid.CreateVersion7();

        // When
        reservation.ChangeSeat(newSeatId);

        // Then
        reservation.SeatId.ShouldBe(newSeatId);

        var @event = reservation.PublishedEvent<ReservationSeatChanged>();

        @event.ShouldNotBeNull();
        @event.ShouldBeOfType<ReservationSeatChanged>();
        @event!.ReservationId.ShouldBe(reservation.Id);
        @event.SeatId.ShouldBe(newSeatId);
    }
}
