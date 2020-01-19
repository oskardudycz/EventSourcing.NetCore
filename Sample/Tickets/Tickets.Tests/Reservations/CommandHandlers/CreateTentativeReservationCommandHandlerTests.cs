using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Tickets.Reservations;
using Tickets.Reservations.Commands;
using Tickets.Reservations.Events;
using Tickets.Tests.Extensions;
using Tickets.Tests.Extensions.Reservations;
using Tickets.Tests.Stubs.Ids;
using Tickets.Tests.Stubs.Reservations;
using Tickets.Tests.Stubs.Storage;
using Xunit;

namespace Tickets.Tests.Reservations.CommandHandlers
{
    public class CreateTentativeReservationCommandHandlerTests
    {
        [Fact]
        public async Task ForCreateTentativeReservationCommand_ShouldAddNewReservation()
        {
            // Given
            var repository = new FakeRepository<Reservation>();
            var idGenerator = new FakeAggregateIdGenerator<Reservation>();
            var numberGenerator = new FakeReservationNumberGenerator();

            var commandHandler = new ReservationCommandHandler(
                repository,
                idGenerator,
                numberGenerator
            );

            var command = CreateTentativeReservation.Create(Guid.NewGuid());

            // When
            await commandHandler.Handle(command, CancellationToken.None);

            //Then
            idGenerator.LastGeneratedId.Should().NotBeNull();
            numberGenerator.LastGeneratedNumber.Should().NotBeNull();

            repository.Aggregates.Should().HaveCount(1);

            var reservation = repository.Aggregates.Values.Single();

            reservation
                .IsTentativeReservationWith(
                    idGenerator.LastGeneratedId.Value,
                    numberGenerator.LastGeneratedNumber,
                    command.SeatId
                )
                .HasTentativeReservationCreatedEventWith(
                    idGenerator.LastGeneratedId.Value,
                    numberGenerator.LastGeneratedNumber,
                    command.SeatId
                );
        }
    }
}
