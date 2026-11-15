using System.Text.Json;
using Shouldly;
using V1 = ECommerce.V1;

namespace EventsVersioning.Tests.Upcasters;

public class ChangedStructure
{
    public record Client(
        Guid Id,
        string Name = "Unknown"
    );

    public record ShoppingCartOpened(
        Guid ShoppingCartId,
        Client Client
    );

    public static ShoppingCartOpened Upcast(
        V1.ShoppingCartOpened oldEvent
    ) =>
        new(
            oldEvent.ShoppingCartId,
            new Client(oldEvent.ClientId)
        );

    public static ShoppingCartOpened Upcast(
        string oldEventJson
    )
    {
        var oldEvent = JsonDocument.Parse(oldEventJson).RootElement;

        return new ShoppingCartOpened(
            oldEvent.GetProperty("ShoppingCartId").GetGuid(),
            new Client(
                oldEvent.GetProperty("ClientId").GetGuid()
            )
        );
    }

    [Fact]
    public void UpcastObjects_Should_BeForwardCompatible()
    {
        // Given
        var oldEvent = new V1.ShoppingCartOpened(Guid.CreateVersion7(), Guid.CreateVersion7());

        // When
        var @event = Upcast(oldEvent);

        @event.ShouldNotBeNull();
        @event.ShoppingCartId.ShouldBe(oldEvent.ShoppingCartId);
        @event.Client.Id.ShouldBe(oldEvent.ClientId);
        @event.Client.Name.ShouldBe("Unknown");
    }

    [Fact]
    public void UpcastJson_Should_BeForwardCompatible()
    {
        // Given
        var oldEvent = new V1.ShoppingCartOpened(Guid.CreateVersion7(), Guid.CreateVersion7());

        // When
        var @event = Upcast(
            JsonSerializer.Serialize(oldEvent)
        );

        @event.ShouldNotBeNull();
        @event.ShoppingCartId.ShouldBe(oldEvent.ShoppingCartId);
        @event.Client.Id.ShouldBe(oldEvent.ClientId);
        @event.Client.Name.ShouldBe("Unknown");
    }
}
