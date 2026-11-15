using Shouldly;
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
        reservation.Status.ShouldBe(ReservationStatus.Tentative);

        reservation.Id.ShouldBe(id);
        reservation.Number.ShouldBe(number);
        reservation.SeatId.ShouldBe(seatId);

        return reservation;
    }

    public static Reservation HasTentativeReservationCreatedEventWith(
        this Reservation reservation,
        Guid id,
        string number,
        Guid seatId)
    {
        var @event = reservation.PublishedEvent<TentativeReservationCreated>();

        @event.ShouldNotBeNull();
        @event.ShouldBeOfType<TentativeReservationCreated>();
        @event!.ReservationId.ShouldBe(id);
        @event.Number.ShouldBe(number);
        @event.SeatId.ShouldBe(seatId);

        return reservation;
    }
}
