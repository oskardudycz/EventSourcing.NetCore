// using System;
// using FluentAssertions;
// using Shipments.Reservations;
// using Shipments.Reservations.Events;
// using Shipments.Tests.Extensions;
// using Shipments.Tests.Extensions.Reservations;
// using Shipments.Tests.Stubs.Ids;
// using Shipments.Tests.Stubs.Reservations;
// using Xunit;
//
// namespace Shipments.Tests.Reservations
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
