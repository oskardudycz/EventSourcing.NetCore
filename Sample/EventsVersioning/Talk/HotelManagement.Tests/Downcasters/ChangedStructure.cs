using System.Text.Json;
using FluentAssertions;
using HotelManagement.GuestStayAccounts;

namespace HotelManagement.Tests.Downcasters;

using V1 = GuestStayAccountEvent;

public class ChangedStructure
{
    public record Money(
        decimal Amount,
        string Currency = "CHF"
    );

    public record PaymentRecorded(
        string GuestStayAccountId,
        Money Amount,
        DateTimeOffset RecordedAt
    );

    public static V1.PaymentRecorded Downcast(
        PaymentRecorded newEvent
    )
    {
        return new V1.PaymentRecorded(
            newEvent.GuestStayAccountId,
            newEvent.Amount.Amount,
            newEvent.RecordedAt
        );
    }

    public static V1.PaymentRecorded Downcast(
        string newEventJson
    )
    {
        var newEvent = JsonDocument.Parse(newEventJson).RootElement;

        return new V1.PaymentRecorded(
            newEvent.GetProperty("GuestStayAccountId").GetString()!,
            newEvent.GetProperty("Amount").GetProperty("Amount").GetDecimal(),
            newEvent.GetProperty("RecordedAt").GetDateTimeOffset()
        );
    }

    [Fact]
    public void DowncastObjects_Should_BeForwardCompatible()
    {
        // Given
        var newEvent = new PaymentRecorded(
            Guid.NewGuid().ToString(),
            new Money((decimal)Random.Shared.NextDouble(), "USD"),
            DateTimeOffset.Now
        );

        // When
        var @event = Downcast(newEvent);

        @event.Should().NotBeNull();
        @event.GuestStayAccountId.Should().Be(newEvent.GuestStayAccountId);
        @event.Amount.Should().Be(newEvent.Amount.Amount);
    }

    [Fact]
    public void DowncastJson_Should_BeForwardCompatible()
    {
        // Given
        var newEvent = new PaymentRecorded(
            Guid.NewGuid().ToString(),
            new Money((decimal)Random.Shared.NextDouble(), "USD"),
            DateTimeOffset.Now
        );
        // When
        var @event = Downcast(
            JsonSerializer.Serialize(newEvent)
        );

        @event.Should().NotBeNull();
        @event.GuestStayAccountId.Should().Be(newEvent.GuestStayAccountId);
        @event.Amount.Should().Be(newEvent.Amount.Amount);
    }
}
