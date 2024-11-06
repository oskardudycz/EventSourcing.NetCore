using System.Text.Json;
using FluentAssertions;
using V1 = HotelManagement.GuestStayAccounts.GuestStayAccountEvent;

namespace HotelManagement.Tests.Upcasters;

public class NewRequiredPropertyFromMetadata
{
    public record EventMetadata(
        string UserId
    );

    public record PaymentRecorded(
        string GuestStayAccountId,
        decimal Amount,
        DateTimeOffset Now,
        string ClerkId
    );

    public static PaymentRecorded Upcast(
        V1.PaymentRecorded newEvent,
        EventMetadata eventMetadata
    )
    {
        return new PaymentRecorded(
            newEvent.GuestStayAccountId,
            newEvent.Amount,
            newEvent.RecordedAt,
            eventMetadata.UserId
        );
    }

    public static PaymentRecorded Upcast(
        string oldEventJson,
        string eventMetadataJson
    )
    {
        var oldEvent = JsonDocument.Parse(oldEventJson).RootElement;
        var eventMetadata = JsonDocument.Parse(eventMetadataJson).RootElement;

        return new PaymentRecorded(
            oldEvent.GetProperty("GuestStayAccountId").GetString()!,
            oldEvent.GetProperty("Amount").GetDecimal(),
            oldEvent.GetProperty("RecordedAt").GetDateTimeOffset(),
            eventMetadata.GetProperty("UserId").GetString()!
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
        var eventMetadata = new EventMetadata(Guid.NewGuid().ToString());

        // When
        var @event = Upcast(oldEvent, eventMetadata);

        @event.Should().NotBeNull();
        @event.Should().NotBeNull();
        @event.GuestStayAccountId.Should().Be(oldEvent.GuestStayAccountId);
        @event.Amount.Should().Be(oldEvent.Amount);
        @event.ClerkId.Should().Be(eventMetadata.UserId);
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
        var eventMetadata = new EventMetadata(Guid.NewGuid().ToString());

        // When
        var @event = Upcast(
            JsonSerializer.Serialize(oldEvent),
            JsonSerializer.Serialize(eventMetadata)
        );

        @event.Should().NotBeNull();
        @event.GuestStayAccountId.Should().Be(oldEvent.GuestStayAccountId);
        @event.Amount.Should().Be(oldEvent.Amount);
        @event.ClerkId.Should().Be(eventMetadata.UserId);
    }
}
