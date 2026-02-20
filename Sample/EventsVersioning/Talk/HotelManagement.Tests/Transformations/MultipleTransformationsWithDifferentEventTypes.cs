using System.Text.Json;
using FluentAssertions;
using HotelManagement.EventStore;
using V1 = HotelManagement.GuestStayAccounts;

namespace HotelManagement.Tests.Transformations
{
    public record Money(
        decimal Amount,
        string Currency
    );

    namespace V2
    {
        public record PaymentRecorded(
            string GuestStayAccountId,
            Money Amount,
            DateTimeOffset RecordedAt
        );
    }

    public record PaymentRecorded(
        string GuestStayAccountId,
        Money Amount,
        DateTimeOffset RecordedAt,
        string ClerkId
    );

    public class MultipleTransformationsWithDifferentEventTypes
    {
        public static PaymentRecorded UpcastV1(
            JsonDocument oldEventJson
        )
        {
            var oldEvent = oldEventJson.RootElement;

            return new PaymentRecorded(
                oldEvent.GetProperty("GuestStayAccountId").GetString()!,
                new Money(oldEvent.GetProperty("Amount").GetDecimal(), "CHF"),
                oldEvent.GetProperty("RecordedAt").GetDateTimeOffset(),
                ""
            );
        }

        public static PaymentRecorded UpcastV2(
            V2.PaymentRecorded oldEvent
        ) =>
            new(
                oldEvent.GuestStayAccountId,
                oldEvent.Amount,
                oldEvent.RecordedAt,
                ""
            );

        [Fact]
        public void UpcastObjects_Should_BeForwardCompatible()
        {
            // Given
            const string eventTypeV1Name = "payment_recorded_v1";
            const string eventTypeV2Name = "payment_recorded_v2";
            const string eventTypeV3Name = "payment_recorded_v3";

            var mapping = new EventTypeMapping()
                .CustomMap<PaymentRecorded>(
                    eventTypeV1Name,
                    eventTypeV2Name,
                    eventTypeV3Name
                );

            var transformations = new EventTransformations()
                .Register(eventTypeV1Name, UpcastV1)
                .Register<V2.PaymentRecorded, PaymentRecorded>(eventTypeV2Name, UpcastV2);

            var serializer = new EventSerializer(mapping, transformations);

            var eventV1 = new V1.PaymentRecorded(
                Guid.NewGuid().ToString(),
                (decimal)Random.Shared.NextDouble(),
                DateTimeOffset.Now
            );
            var eventV2 = new V2.PaymentRecorded(
                Guid.NewGuid().ToString(),
                new Money((decimal)Random.Shared.NextDouble(), "USD"),
                DateTimeOffset.Now
            );
            var eventV3 = new PaymentRecorded(
                Guid.NewGuid().ToString(),
                new Money((decimal)Random.Shared.NextDouble(), "EUR"),
                DateTimeOffset.Now,
                Guid.NewGuid().ToString()
            );

            var events = new[]
            {
                new SerializedEvent(eventTypeV1Name, JsonSerializer.Serialize(eventV1)),
                new SerializedEvent(eventTypeV2Name, JsonSerializer.Serialize(eventV2)),
                new SerializedEvent(eventTypeV3Name, JsonSerializer.Serialize(eventV3))
            };

            // When
            var deserializedEvents = events
                .Select(serializer.Deserialize)
                .OfType<PaymentRecorded>()
                .ToList();

            deserializedEvents.Should().HaveCount(3);

            // Then
            deserializedEvents[0].GuestStayAccountId.Should().Be(eventV1.GuestStayAccountId);
            deserializedEvents[0].Amount.Should().Be(new Money(eventV1.Amount, "CHF"));
            deserializedEvents[0].ClerkId.Should().Be("");
            deserializedEvents[0].RecordedAt.Should().Be(eventV1.RecordedAt);


            deserializedEvents[1].GuestStayAccountId.Should().Be(eventV2.GuestStayAccountId);
            deserializedEvents[1].Amount.Should().Be(eventV2.Amount);
            deserializedEvents[1].ClerkId.Should().Be("");
            deserializedEvents[1].RecordedAt.Should().Be(eventV2.RecordedAt);


            deserializedEvents[2].GuestStayAccountId.Should().Be(eventV3.GuestStayAccountId);
            deserializedEvents[2].Amount.Should().Be(eventV3.Amount);
            deserializedEvents[2].ClerkId.Should().Be(eventV3.ClerkId);
            deserializedEvents[2].RecordedAt.Should().Be(eventV3.RecordedAt);
        }
    }
}
