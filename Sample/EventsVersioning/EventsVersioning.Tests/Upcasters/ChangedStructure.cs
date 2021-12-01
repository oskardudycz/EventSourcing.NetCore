using System;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using V1 = ECommerce.V1;

namespace EventsVersioning.Tests.Upcasters;

public class ChangedStructure
{
    public record Client(
        Guid Id,
        string Name = "Unknown"
    );

    public record ShoppingCartInitialized(
        Guid ShoppingCartId,
        Client Client
    );

    public static ShoppingCartInitialized Upcast(
        V1.ShoppingCartInitialized oldEvent
    )
    {
        return new ShoppingCartInitialized(
            oldEvent.ShoppingCartId,
            new Client(oldEvent.ClientId)
        );
    }

    public static ShoppingCartInitialized Upcast(
        string oldEventJson
    )
    {
        var oldEvent = JsonDocument.Parse(oldEventJson).RootElement;

        return new ShoppingCartInitialized(
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
        var oldEvent = new V1.ShoppingCartInitialized(Guid.NewGuid(), Guid.NewGuid());

        // When
        var @event = Upcast(oldEvent);

        @event.Should().NotBeNull();
        @event.ShoppingCartId.Should().Be(oldEvent.ShoppingCartId);
        @event.Client.Id.Should().Be(oldEvent.ClientId);
        @event.Client.Name.Should().Be("Unknown");
    }

    [Fact]
    public void UpcastJson_Should_BeForwardCompatible()
    {
        // Given
        var oldEvent = new V1.ShoppingCartInitialized(Guid.NewGuid(), Guid.NewGuid());

        // When
        var @event = Upcast(
            JsonSerializer.Serialize(oldEvent)
        );

        @event.Should().NotBeNull();
        @event.ShoppingCartId.Should().Be(oldEvent.ShoppingCartId);
        @event.Client.Id.Should().Be(oldEvent.ClientId);
        @event.Client.Name.Should().Be("Unknown");
    }
}