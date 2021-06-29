// using FluentAssertions;
// using Payments.Reservations;
// using Payments.Reservations.Events;
// using Payments.Tests.Builders;
// using Payments.Tests.Extensions;
// using Xunit;
//
// namespace Payments.Tests.Reservations
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
