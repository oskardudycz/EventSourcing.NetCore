using System;
using System.Text.Json.Serialization;
using FluentAssertions;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;
using V1 = ECommerce.V1;

namespace EventsVersioning.Tests.SimpleMappings;

public class RenamedProperty
{
    public class ShoppingCartInitialized
    {
        [JsonPropertyName("ShoppingCartId")]
        public Guid CartId { get; init; }
        public Guid ClientId { get; init; }

        public ShoppingCartInitialized(
            Guid cartId,
            Guid clientId
        )
        {
            CartId = cartId;
            ClientId = clientId;
        }
    }

    [Fact]
    public void Should_BeForwardCompatible()
    {
        // Given
        var oldEvent = new V1.ShoppingCartInitialized(Guid.NewGuid(), Guid.NewGuid());
        var json = JsonSerializer.Serialize(oldEvent);

        // When
        var @event = JsonSerializer.Deserialize<ShoppingCartInitialized>(json);

        @event.Should().NotBeNull();
        @event!.CartId.Should().Be(oldEvent.ShoppingCartId);
        @event.ClientId.Should().Be(oldEvent.ClientId);
    }

    [Fact]
    public void Should_BeBackwardCompatible()
    {
        // Given
        var @event = new ShoppingCartInitialized(Guid.NewGuid(), Guid.NewGuid());
        var json = JsonSerializer.Serialize(@event);

        // When
        var oldEvent = JsonSerializer.Deserialize<V1.ShoppingCartInitialized>(json);

        oldEvent.Should().NotBeNull();
        oldEvent!.ShoppingCartId.Should().Be(@event.CartId);
        oldEvent.ClientId.Should().Be(@event.ClientId);
    }
}
