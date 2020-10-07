// using System;
// using FluentAssertions;
// using Carts.Reservations;
// using Carts.Reservations.Events;
// using Carts.Tests.Extensions;
// using Carts.Tests.Extensions.Reservations;
// using Carts.Tests.Stubs.Ids;
// using Carts.Tests.Stubs.Reservations;
// using Xunit;
//
// namespace Carts.Tests.Reservations
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
