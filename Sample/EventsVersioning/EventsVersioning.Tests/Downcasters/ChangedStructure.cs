using System;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using V1 = ECommerce.V1;

namespace EventsVersioning.Tests.Downcasters;

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

public static V1.ShoppingCartInitialized Downcast(
    ShoppingCartInitialized newEvent
)
{
    return new V1.ShoppingCartInitialized(
        newEvent.ShoppingCartId,
        newEvent.Client.Id
    );
}

public static V1.ShoppingCartInitialized Downcast(
    string newEventJson
)
{
    var newEvent = JsonDocument.Parse(newEventJson).RootElement;

    return new V1.ShoppingCartInitialized(
        newEvent.GetProperty("ShoppingCartId").GetGuid(),
        newEvent.GetProperty("Client").GetProperty("Id").GetGuid()
    );
}

    [Fact]
    public void UpcastObjects_Should_BeForwardCompatible()
    {
        // Given
        var newEvent = new ShoppingCartInitialized(
            Guid.NewGuid(),
            new Client( Guid.NewGuid(), "Oskar the Grouch")
        );

        // When
        var @event = Downcast(newEvent);

        @event.Should().NotBeNull();
        @event.ShoppingCartId.Should().Be(newEvent.ShoppingCartId);
        @event.ClientId.Should().Be(newEvent.Client.Id);
    }

    [Fact]
    public void UpcastJson_Should_BeForwardCompatible()
    {
        // Given
        var newEvent = new ShoppingCartInitialized(
            Guid.NewGuid(),
            new Client( Guid.NewGuid(), "Oskar the Grouch")
        );
        // When
        var @event = Downcast(
            JsonSerializer.Serialize(newEvent)
        );

        @event.Should().NotBeNull();
        @event.ShoppingCartId.Should().Be(newEvent.ShoppingCartId);
        @event.ClientId.Should().Be(newEvent.Client.Id);
    }
}
