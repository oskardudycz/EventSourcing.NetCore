using System.Text.Json;
using Shouldly;
using V1 = ECommerce.V1;

namespace EventsVersioning.Tests.SimpleMappings;

public class NewRequiredProperty
{
    public enum ShoppingCartStatus
    {
        Pending = 1,
        Opened = 2,
        Confirmed = 3,
        Cancelled = 4
    }

    public record ShoppingCartOpened(
        Guid ShoppingCartId,
        Guid ClientId,
        // Adding new not required property as nullable
        ShoppingCartStatus Status = ShoppingCartStatus.Opened
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
        @event.Status.ShouldBe(ShoppingCartStatus.Opened);
    }

    [Fact]
    public void Should_BeBackwardCompatible()
    {
        // Given
        var @event = new ShoppingCartOpened(Guid.CreateVersion7(), Guid.CreateVersion7(), ShoppingCartStatus.Pending);
        var json = JsonSerializer.Serialize(@event);

        // When
        var oldEvent = JsonSerializer.Deserialize<V1.ShoppingCartOpened>(json);

        oldEvent.ShouldNotBeNull();
        oldEvent!.ShoppingCartId.ShouldBe(@event.ShoppingCartId);
        oldEvent.ClientId.ShouldBe(@event.ClientId);
    }
}
