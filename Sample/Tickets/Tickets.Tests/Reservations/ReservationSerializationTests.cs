using System.Text.Json;
using FluentAssertions;
using Tickets.Reservations;
using Tickets.Tests.Stubs.Reservations;
using Xunit;

namespace Tickets.Tests.Reservations;

public class ReservationSerializationTests
{
    [Fact]
    public void SnapshotRoundTripPreservesIdentityAndConfirmedReservation()
    {
        var reservation = Reservation.CreateTentative(
            Guid.NewGuid(), new FakeReservationNumberGenerator(), Guid.NewGuid());
        reservation.Confirm();

        var restored = JsonSerializer.Deserialize<Reservation>(JsonSerializer.Serialize(reservation))!;

        restored.Id.Should().Be(reservation.Id);
        restored.SeatId.Should().Be(reservation.SeatId);
        restored.Number.Should().Be(reservation.Number);
        restored.Status.Should().Be(ReservationStatus.Confirmed);
        restored.DequeueUncommittedEvents().Should().BeEmpty();
    }
}
