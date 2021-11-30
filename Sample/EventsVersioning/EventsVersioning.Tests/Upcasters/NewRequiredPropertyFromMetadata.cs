using System;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using V1 = ECommerce.V1;

namespace EventsVersioning.Tests.Upcasters
{
    public class NewRequiredPropertyFromMetadata
    {
        public record EventMetadata(
            Guid UserId
        );

        public record ShoppingCartInitialized(
            Guid ShoppingCartId,
            Guid ClientId,
            Guid InitializedBy
        );

        public static ShoppingCartInitialized Upcast(
            V1.ShoppingCartInitialized oldEvent,
            EventMetadata eventMetadata
        )
        {
            return new ShoppingCartInitialized(
                oldEvent.ShoppingCartId,
                oldEvent.ClientId,
                eventMetadata.UserId
            );
        }

        public static ShoppingCartInitialized Upcast(
            string oldEventJson,
            string eventMetadataJson
        )
        {
            var oldEvent = JsonDocument.Parse(oldEventJson);
            var eventMetadata = JsonDocument.Parse(eventMetadataJson);

            return new ShoppingCartInitialized(
                oldEvent.RootElement.GetProperty("ShoppingCartId").GetGuid(),
                oldEvent.RootElement.GetProperty("ClientId").GetGuid(),
                eventMetadata.RootElement.GetProperty("UserId").GetGuid()
            );
        }

        [Fact]
        public void UpcastObjects_Should_BeForwardCompatible()
        {
            // Given
            var oldEvent = new V1.ShoppingCartInitialized(Guid.NewGuid(), Guid.NewGuid());
            var eventMetadata = new EventMetadata(Guid.NewGuid());

            // When
            var @event = Upcast(oldEvent, eventMetadata);

            @event.Should().NotBeNull();
            @event.ShoppingCartId.Should().Be(oldEvent.ShoppingCartId);
            @event.ClientId.Should().Be(oldEvent.ClientId);
            @event.InitializedBy.Should().Be(eventMetadata.UserId);
        }

        [Fact]
        public void UpcastJson_Should_BeForwardCompatible()
        {
            // Given
            var oldEvent = new V1.ShoppingCartInitialized(Guid.NewGuid(), Guid.NewGuid());
            var eventMetadata = new EventMetadata(Guid.NewGuid());

            // When
            var @event = Upcast(
                JsonSerializer.Serialize(oldEvent),
                JsonSerializer.Serialize(eventMetadata)
            );

            @event.Should().NotBeNull();
            @event.ShoppingCartId.Should().Be(oldEvent.ShoppingCartId);
            @event.ClientId.Should().Be(oldEvent.ClientId);
            @event.InitializedBy.Should().Be(eventMetadata.UserId);
        }
    }
}
