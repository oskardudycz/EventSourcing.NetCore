// using System;
// using FluentAssertions;
// using Orders.Reservations;
// using Orders.Reservations.Events;
// using Orders.Tests.Builders;
// using Orders.Tests.Extensions;
// using Xunit;
//
// namespace Orders.Tests.Reservations
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
