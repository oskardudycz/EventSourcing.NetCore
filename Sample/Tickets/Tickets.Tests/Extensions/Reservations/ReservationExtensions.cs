using FluentAssertions;
using Tickets.Reservations;
using Tickets.Reservations.CreatingTentativeReservation;

namespace Tickets.Tests.Extensions.Reservations;

internal static class ReservationExtensions
{
    public static Reservation IsTentativeReservationWith(
        this Reservation reservation,
        Guid id,
        string number,
        Guid seatId)
    {
        reservation.Status.Should().Be(ReservationStatus.Tentative);

        reservation.Id.Should().Be(id);
        reservation.Number.Should().Be(number);
        reservation.SeatId.Should().Be(seatId);

        return reservation;
    }

    public static Reservation HasTentativeReservationCreatedEventWith(
        this Reservation reservation,
        Guid id,
        string number,
        Guid seatId)
    {
        var @event = reservation.PublishedEvent<TentativeReservationCreated>();

        @event.Should().NotBeNull();
        @event.Should().BeOfType<TentativeReservationCreated>();
        @event!.ReservationId.Should().Be(id);
        @event.Number.Should().Be(number);
        @event.SeatId.Should().Be(seatId);

        return reservation;
    }
}
