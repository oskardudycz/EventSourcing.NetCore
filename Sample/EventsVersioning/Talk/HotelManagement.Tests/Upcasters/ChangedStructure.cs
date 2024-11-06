using System.Text.Json;
using FluentAssertions;
using V1 = HotelManagement.GuestStayAccounts.GuestStayAccountEvent;

namespace HotelManagement.Tests.Upcasters;

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

    public static PaymentRecorded Upcast(
        V1.PaymentRecorded newEvent
    )
    {
        return new PaymentRecorded(
            newEvent.GuestStayAccountId,
            new Money(newEvent.Amount),
            newEvent.RecordedAt
        );
    }

    public static PaymentRecorded Upcast(
        string oldEventJson
    )
    {
        var oldEvent = JsonDocument.Parse(oldEventJson).RootElement;

        return new PaymentRecorded(
            oldEvent.GetProperty("GuestStayAccountId").GetString()!,
            new Money(oldEvent.GetProperty("Amount").GetDecimal()),
            oldEvent.GetProperty("RecordedAt").GetDateTimeOffset()
        );
    }

    [Fact]
    public void UpcastObjects_Should_BeForwardCompatible()
    {
        // Given
        var oldEvent = new V1.PaymentRecorded(
            Guid.NewGuid().ToString(),
            (decimal)Random.Shared.NextDouble(),
            DateTimeOffset.Now
        );

        // When
        var @event = Upcast(oldEvent);

        @event.Should().NotBeNull();
        @event.GuestStayAccountId.Should().Be(oldEvent.GuestStayAccountId);
        @event.Amount.Should().Be(new Money(oldEvent.Amount, "CHF"));
    }

    [Fact]
    public void UpcastJson_Should_BeForwardCompatible()
    {
        // Given
        var oldEvent = new V1.PaymentRecorded(
            Guid.NewGuid().ToString(),
            (decimal)Random.Shared.NextDouble(),
            DateTimeOffset.Now
        );

        // When
        var @event = Upcast(
            JsonSerializer.Serialize(oldEvent)
        );

        @event.Should().NotBeNull();
        @event.GuestStayAccountId.Should().Be(oldEvent.GuestStayAccountId);
        @event.Amount.Should().Be(new Money(oldEvent.Amount, "CHF"));
    }
}
