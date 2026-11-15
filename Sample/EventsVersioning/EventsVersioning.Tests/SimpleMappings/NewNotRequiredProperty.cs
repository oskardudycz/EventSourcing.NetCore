using System.Text.Json;
using Shouldly;
using V1 = ECommerce.V1;

namespace EventsVersioning.Tests.SimpleMappings;

public class NewNotRequiredProperty
{
    public record ShoppingCartOpened(
        Guid ShoppingCartId,
        Guid ClientId,
        // Adding new not required property as nullable
        DateTime? InitializedAt
    );

    [Fact]
    public void Should_BeForwardCompatible()
    {
        // Given
        var oldEvent = new V1.ShoppingCartOpened(Guid.CreateVersion7(), Guid.CreateVersion7());
        var json = JsonSerializer.Serialize(oldEvent);

        // When
        var @event = JsonSerializer.Deserialize<ShoppingCartOpened>(json);

        @event.ShouldNotBeNull();
        @event!.ShoppingCartId.ShouldBe(oldEvent.ShoppingCartId);
        @event.ClientId.ShouldBe(oldEvent.ClientId);
        @event.InitializedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_BeBackwardCompatible()
    {
        // Given
        var @event = new ShoppingCartOpened(Guid.CreateVersion7(), Guid.CreateVersion7(), DateTime.UtcNow);
        var json = JsonSerializer.Serialize(@event);

        // When
        var oldEvent = JsonSerializer.Deserialize<V1.ShoppingCartOpened>(json);

        oldEvent.ShouldNotBeNull();
        oldEvent!.ShoppingCartId.ShouldBe(@event.ShoppingCartId);
        oldEvent.ClientId.ShouldBe(@event.ClientId);
    }
}
