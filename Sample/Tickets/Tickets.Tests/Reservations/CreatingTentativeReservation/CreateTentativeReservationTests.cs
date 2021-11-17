using System;
using FluentAssertions;
using Tickets.Reservations;
using Tickets.Tests.Extensions.Reservations;
using Tickets.Tests.Stubs.Reservations;
using Xunit;

namespace Tickets.Tests.Reservations.CreatingTentativeReservation;

public class CreateTentativeReservationTests
{
    [Fact]
    public void ForValidParams_ShouldCreateReservationWithTentativeStatus()
    {
        // Given
        var reservationId = Guid.NewGuid();
        var numberGenerator = new FakeReservationNumberGenerator();
        var seatId = Guid.NewGuid();

        // When
        var reservation = Reservation.CreateTentative(
            reservationId,
            numberGenerator,
            seatId
        );

        // Then
        numberGenerator.LastGeneratedNumber.Should().NotBeNull();

        reservation
            .IsTentativeReservationWith(
                reservationId,
                numberGenerator.LastGeneratedNumber,
                seatId
            )
            .HasTentativeReservationCreatedEventWith(
                reservationId,
                numberGenerator.LastGeneratedNumber,
                seatId
            );
    }
}