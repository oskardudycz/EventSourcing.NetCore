using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using V1 = HotelManagement.GuestStayAccounts;

namespace HotelManagement.Tests.SimpleMappings;

public class RenamedProperty
{
    public record PaymentRecorded(
        [property: JsonPropertyName("GuestStayAccountId")]
        string AccountId,
        decimal Amount,
        DateTimeOffset RecordedAt,
        string Currency = PaymentRecorded.DefaultCurrency
    )
    {
        public const string DefaultCurrency = "CHF";
    }

    [Fact]
    public void Should_BeForwardCompatible()
    {
        // Given
        var oldEvent = new V1.PaymentRecorded(
            Guid.NewGuid().ToString(),
            (decimal)Random.Shared.NextDouble(),
            DateTimeOffset.Now
        );
        var json = JsonSerializer.Serialize(oldEvent);

        // When
        var @event = JsonSerializer.Deserialize<PaymentRecorded>(json);

        @event.Should().NotBeNull();
        @event!.AccountId.Should().Be(oldEvent.GuestStayAccountId);
        @event.Amount.Should().Be(oldEvent.Amount);
        @event.Currency.Should().Be(PaymentRecorded.DefaultCurrency);
    }

    [Fact]
    public void Should_BeBackwardCompatible()
    {
        // Given
        var @event = new PaymentRecorded(
            Guid.NewGuid().ToString(),
            (decimal)Random.Shared.NextDouble(),
            DateTimeOffset.Now,
            Guid.NewGuid().ToString()
        );
        var json = JsonSerializer.Serialize(@event);

        // When
        var oldEvent = JsonSerializer.Deserialize<V1.PaymentRecorded>(json);

        oldEvent.Should().NotBeNull();
        oldEvent!.GuestStayAccountId.Should().Be(@event.AccountId);
        oldEvent.Amount.Should().Be(@event.Amount);
    }
}
