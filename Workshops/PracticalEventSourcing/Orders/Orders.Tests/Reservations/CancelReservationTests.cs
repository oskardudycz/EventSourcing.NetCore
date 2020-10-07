// using FluentAssertions;
// using Orders.Reservations;
// using Orders.Reservations.Events;
// using Orders.Tests.Builders;
// using Orders.Tests.Extensions;
// using Xunit;
//
// namespace Orders.Tests.Reservations
// {
//     public class CancelReservationTests
//     {
//         [Fact]
//         public void ForTentativeReservation_ShouldSucceed()
//         {
//             // Given
//             var reservation = ReservationBuilder
//                 .Create()
//                 .Tentative()
//                 .Build();
//
//             // When
//             reservation.Cancel();
//
//             // Then
//             reservation.Status.Should().Be(ReservationStatus.Cancelled);
//             reservation.Version.Should().Be(2);
//
//             var @event = reservation.PublishedEvent<ReservationCancelled>();
//
//             @event.Should().BeOfType<ReservationCancelled>();
//             @event.ReservationId.Should().Be(reservation.Id);
//         }
//
//         [Fact]
//         public void ForConfirmedReservation_ShouldFailWithInvalidOperation()
//         {
//
//         }
//
//         [Fact]
//         public void ForCancelledReservation_ShouldFailWithInvalidOperation()
//         {
//
//         }
//     }
// }
