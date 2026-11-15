using System.Text.Json;
using Shouldly;
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

        restored.Id.ShouldBe(reservation.Id);
        restored.SeatId.ShouldBe(reservation.SeatId);
        restored.Number.ShouldBe(reservation.Number);
        restored.Status.ShouldBe(ReservationStatus.Confirmed);
        restored.DequeueUncommittedEvents().ShouldBeEmpty();
    }
}
