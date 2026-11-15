using System.Text.Json;
using Shouldly;
using V1 = ECommerce.V1;

namespace EventsVersioning.Tests.Upcasters;

public class NewRequiredPropertyFromMetadata
{
    public record EventMetadata(
        Guid UserId
    );

    public record ShoppingCartOpened(
        Guid ShoppingCartId,
        Guid ClientId,
        Guid InitializedBy
    );

    public static ShoppingCartOpened Upcast(
        V1.ShoppingCartOpened oldEvent,
        EventMetadata eventMetadata
    ) =>
        new(
            oldEvent.ShoppingCartId,
            oldEvent.ClientId,
            eventMetadata.UserId
        );

    public static ShoppingCartOpened Upcast(
        string oldEventJson,
        string eventMetadataJson
    )
    {
        var oldEvent = JsonDocument.Parse(oldEventJson);
        var eventMetadata = JsonDocument.Parse(eventMetadataJson);

        return new ShoppingCartOpened(
            oldEvent.RootElement.GetProperty("ShoppingCartId").GetGuid(),
            oldEvent.RootElement.GetProperty("ClientId").GetGuid(),
            eventMetadata.RootElement.GetProperty("UserId").GetGuid()
        );
    }

    [Fact]
    public void UpcastObjects_Should_BeForwardCompatible()
    {
        // Given
        var oldEvent = new V1.ShoppingCartOpened(Guid.CreateVersion7(), Guid.CreateVersion7());
        var eventMetadata = new EventMetadata(Guid.CreateVersion7());

        // When
        var @event = Upcast(oldEvent, eventMetadata);

        @event.ShouldNotBeNull();
        @event.ShoppingCartId.ShouldBe(oldEvent.ShoppingCartId);
        @event.ClientId.ShouldBe(oldEvent.ClientId);
        @event.InitializedBy.ShouldBe(eventMetadata.UserId);
    }

    [Fact]
    public void UpcastJson_Should_BeForwardCompatible()
    {
        // Given
        var oldEvent = new V1.ShoppingCartOpened(Guid.CreateVersion7(), Guid.CreateVersion7());
        var eventMetadata = new EventMetadata(Guid.CreateVersion7());

        // When
        var @event = Upcast(
            JsonSerializer.Serialize(oldEvent),
            JsonSerializer.Serialize(eventMetadata)
        );

        @event.ShouldNotBeNull();
        @event.ShoppingCartId.ShouldBe(oldEvent.ShoppingCartId);
        @event.ClientId.ShouldBe(oldEvent.ClientId);
        @event.InitializedBy.ShouldBe(eventMetadata.UserId);
    }
}
