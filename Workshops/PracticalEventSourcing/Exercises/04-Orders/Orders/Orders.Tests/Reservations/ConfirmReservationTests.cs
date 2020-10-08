// using FluentAssertions;
// using Orders.Reservations;
// using Orders.Reservations.Events;
// using Orders.Tests.Builders;
// using Orders.Tests.Extensions;
// using Xunit;
//
// namespace Orders.Tests.Reservations
// {
//     public class ConfirmReservationTests
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
//             reservation.Confirm();
//
//             // Then
//             reservation.Status.Should().Be(ReservationStatus.Confirmed);
//             reservation.Version.Should().Be(2);
//
//             var @event = reservation.PublishedEvent<ReservationConfirmed>();
//
//             @event.Should().BeOfType<ReservationConfirmed>();
//             @event.ReservationId.Should().Be(reservation.Id);
//         }
//     }
// }
