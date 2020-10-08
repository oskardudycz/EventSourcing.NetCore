// using System;
// using FluentAssertions;
// using Payments.Reservations;
// using Payments.Reservations.Events;
//
// namespace Payments.Tests.Extensions.Reservations
// {
//     internal static class ReservationExtensions
//     {
//         public static Reservation IsTentativeReservationWith(
//             this Reservation reservation,
//             Guid id,
//             string number,
//             Guid seatId)
//         {
//             reservation.Status.Should().Be(ReservationStatus.Tentative);
//
//             reservation.Id.Should().Be(id);
//             reservation.Number.Should().Be(number);
//             reservation.SeatId.Should().Be(seatId);
//             reservation.Version.Should().Be(1);
//
//             return reservation;
//         }
//
//         public static Reservation HasTentativeReservationCreatedEventWith(
//             this Reservation reservation,
//             Guid id,
//             string number,
//             Guid seatId)
//         {
//             var @event = reservation.PublishedEvent<TentativeReservationCreated>();
//
//             @event.Should().BeOfType<TentativeReservationCreated>();
//             @event.ReservationId.Should().Be(id);
//             @event.Number.Should().Be(number);
//             @event.SeatId.Should().Be(seatId);
//
//             return reservation;
//         }
//     }
// }
