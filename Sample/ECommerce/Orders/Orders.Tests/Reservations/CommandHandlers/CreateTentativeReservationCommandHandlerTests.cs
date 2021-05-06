// using System;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using FluentAssertions;
// using Orders.Reservations;
// using Orders.Reservations.Commands;
// using Orders.Tests.Extensions.Reservations;
// using Orders.Tests.Stubs.Reservations;
// using Orders.Tests.Stubs.Storage;
// using Xunit;
//
// namespace Orders.Tests.Reservations.CommandHandlers
// {
//     public class CreateTentativeReservationCommandHandlerTests
//     {
//         [Fact]
//         public async Task ForCreateTentativeReservationCommand_ShouldAddNewReservation()
//         {
//             // Given
//             var repository = new FakeRepository<Reservation>();
//             var numberGenerator = new FakeReservationNumberGenerator();
//
//             var commandHandler = new ReservationCommandHandler(
//                 repository,
//                 numberGenerator
//             );
//
//             var command = CreateTentativeReservation.Create(Guid.NewGuid(), Guid.NewGuid());
//
//             // When
//             await commandHandler.Handle(command, CancellationToken.None);
//
//             //Then
//             repository.Aggregates.Should().HaveCount(1);
//
//             var reservation = repository.Aggregates.Values.Single();
//
//             reservation
//                 .IsTentativeReservationWith(
//                     command.ReservationId,
//                     numberGenerator.LastGeneratedNumber,
//                     command.SeatId
//                 )
//                 .HasTentativeReservationCreatedEventWith(
//                     command.ReservationId,
//                     numberGenerator.LastGeneratedNumber,
//                     command.SeatId
//                 );
//         }
//     }
// }
