// using System;
// using FluentAssertions;
// using Carts.Reservations;
// using Carts.Reservations.Events;
// using Carts.Tests.Builders;
// using Carts.Tests.Extensions;
// using Xunit;
//
// namespace Carts.Tests.Reservations
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
