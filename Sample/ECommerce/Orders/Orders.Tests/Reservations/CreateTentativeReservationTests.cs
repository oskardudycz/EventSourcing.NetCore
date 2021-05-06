// using System;
// using FluentAssertions;
// using Orders.Reservations;
// using Orders.Reservations.Events;
// using Orders.Tests.Extensions;
// using Orders.Tests.Extensions.Reservations;
// using Orders.Tests.Stubs.Ids;
// using Orders.Tests.Stubs.Reservations;
// using Xunit;
//
// namespace Orders.Tests.Reservations
// {
//     public class CreateTentativeReservationTests
//     {
//         [Fact]
//         public void ForValidParams_ShouldCreateReservationWithTentativeStatus()
//         {
//             // Given
//             var reservationId = Guid.NewGuid();
//             var numberGenerator = new FakeReservationNumberGenerator();
//             var seatId = Guid.NewGuid();
//
//             // When
//             var reservation = Reservation.CreateTentative(
//                 reservationId,
//                 numberGenerator,
//                 seatId
//             );
//
//             // Then
//             numberGenerator.LastGeneratedNumber.Should().NotBeNull();
//
//             reservation
//                 .IsTentativeReservationWith(
//                     reservationId,
//                     numberGenerator.LastGeneratedNumber,
//                     seatId
//                 )
//                 .HasTentativeReservationCreatedEventWith(
//                     reservationId,
//                     numberGenerator.LastGeneratedNumber,
//                     seatId
//                 );
//         }
//     }
// }
