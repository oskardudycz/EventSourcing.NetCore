// using System;
// using FluentAssertions;
// using Payments.Reservations;
// using Payments.Reservations.Events;
// using Payments.Tests.Extensions;
// using Payments.Tests.Extensions.Reservations;
// using Payments.Tests.Stubs.Ids;
// using Payments.Tests.Stubs.Reservations;
// using Xunit;
//
// namespace Payments.Tests.Reservations
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
