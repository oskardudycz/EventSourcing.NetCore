using System.Text.Json.Serialization;
using FluentAssertions;
using JsonSerializer = System.Text.Json.JsonSerializer;
using V1 = ECommerce.V1;

namespace EventsVersioning.Tests.SimpleMappings;

public class RenamedProperty
{
    public class ShoppingCartOpened(
        Guid cartId,
        Guid clientId)
    {
        [JsonPropertyName("ShoppingCartId")]
        public Guid CartId { get; init; } = cartId;

        public Guid ClientId { get; init; } = clientId;
    }

    [Fact]
    public void Should_BeForwardCompatible()
    {
        // Given
        var oldEvent = new V1.ShoppingCartOpened(Guid.CreateVersion7(), Guid.CreateVersion7());
        var json = JsonSerializer.Serialize(oldEvent);

        // When
        var @event = JsonSerializer.Deserialize<ShoppingCartOpened>(json);

        @event.Should().NotBeNull();
        @event!.CartId.Should().Be(oldEvent.ShoppingCartId);
        @event.ClientId.Should().Be(oldEvent.ClientId);
    }

    [Fact]
    public void Should_BeBackwardCompatible()
    {
        // Given
        var @event = new ShoppingCartOpened(Guid.CreateVersion7(), Guid.CreateVersion7());
        var json = JsonSerializer.Serialize(@event);

        // When
        var oldEvent = JsonSerializer.Deserialize<V1.ShoppingCartOpened>(json);

        oldEvent.Should().NotBeNull();
        oldEvent!.ShoppingCartId.Should().Be(@event.CartId);
        oldEvent.ClientId.Should().Be(@event.ClientId);
    }
}
