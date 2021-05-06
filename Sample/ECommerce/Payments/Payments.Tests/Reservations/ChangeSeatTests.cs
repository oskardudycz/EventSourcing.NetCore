// using System;
// using FluentAssertions;
// using Payments.Reservations;
// using Payments.Reservations.Events;
// using Payments.Tests.Builders;
// using Payments.Tests.Extensions;
// using Xunit;
//
// namespace Payments.Tests.Reservations
// {
//     public class ChangeSeatTests
//     {
//         [Fact]
//         public void ForValidParams_UpdatesSeatId()
//         {
//             // Given
//             var reservation = ReservationBuilder
//                 .Create()
//                 .Tentative()
//                 .Build();
//
//             var newSeatId = Guid.NewGuid();
//
//             // When
//             reservation.ChangeSeat(newSeatId);
//
//             // Then
//             reservation.SeatId.Should().Be(newSeatId);
//             reservation.Version.Should().Be(2);
//
//             var @event = reservation.PublishedEvent<ReservationSeatChanged>();
//
//             @event.Should().BeOfType<ReservationSeatChanged>();
//             @event.ReservationId.Should().Be(reservation.Id);
//             @event.SeatId.Should().Be(newSeatId);
//         }
//     }
// }
