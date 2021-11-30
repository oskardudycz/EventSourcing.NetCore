using System;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using V1 = ECommerce.V1;

namespace EventsVersioning.Tests.SimpleMappings
{
    public class NewNotRequiredProperty
    {
        public record ShoppingCartInitialized(
            Guid ShoppingCartId,
            Guid ClientId,
            // Adding new not required property as nullable
            DateTime? IntializedAt
        );

        [Fact]
        public void Should_BeForwardCompatible()
        {
            // Given
            var oldEvent = new V1.ShoppingCartInitialized(Guid.NewGuid(), Guid.NewGuid());
            var json = JsonSerializer.Serialize(oldEvent);

            // When
            var @event = JsonSerializer.Deserialize<ShoppingCartInitialized>(json);

            @event.Should().NotBeNull();
            @event!.ShoppingCartId.Should().Be(oldEvent.ShoppingCartId);
            @event.ClientId.Should().Be(oldEvent.ClientId);
            @event.IntializedAt.Should().BeNull();
        }

        [Fact]
        public void Should_BeBackwardCompatible()
        {
            // Given
            var @event = new ShoppingCartInitialized(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
            var json = JsonSerializer.Serialize(@event);

            // When
            var oldEvent = JsonSerializer.Deserialize<V1.ShoppingCartInitialized>(json);

            oldEvent.Should().NotBeNull();
            oldEvent!.ShoppingCartId.Should().Be(@event.ShoppingCartId);
            oldEvent.ClientId.Should().Be(@event.ClientId);
        }
    }
}
