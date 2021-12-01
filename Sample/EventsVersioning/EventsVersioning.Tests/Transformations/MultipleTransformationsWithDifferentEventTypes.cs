using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using V1 = ECommerce.V1;

namespace EventsVersioning.Tests.Transformations;

public class MultipleTransformationsWithDifferentEventTypes
{
    public record Client(
        Guid Id,
        string Name = "Unknown"
    );

    public record ShoppingCartInitialized(
        Guid ShoppingCartId,
        Client Client
    );

    public enum ShoppingCartStatus
    {
        Pending = 1,
        Initialized = 2,
        Confirmed = 3,
        Cancelled = 4
    }

    public record ShoppingCartInitializedWithStatus(
        Guid ShoppingCartId,
        Client Client,
        ShoppingCartStatus Status
    );

    public static ShoppingCartInitializedWithStatus UpcastV1(
        JsonDocument oldEventJson
    )
    {
        var oldEvent = oldEventJson.RootElement;

        return new ShoppingCartInitializedWithStatus(
            oldEvent.GetProperty("ShoppingCartId").GetGuid(),
            new Client(
                oldEvent.GetProperty("ClientId").GetGuid()
            ),
            ShoppingCartStatus.Initialized
        );
    }

    public static ShoppingCartInitializedWithStatus UpcastV2(
        ShoppingCartInitialized oldEvent
    )
    {
        return new ShoppingCartInitializedWithStatus(
            oldEvent.ShoppingCartId,
            oldEvent.Client,
            ShoppingCartStatus.Initialized
        );
    }

    public class EventTransformations
    {
        private readonly Dictionary<string, Func<string, object>> jsonTransformations = new();

        public bool TryTransform(string eventTypeName, string json, out object? result)
        {
            if (!jsonTransformations.TryGetValue(eventTypeName, out var transformJson))
            {
                result = null;
                return false;
            }

            result = transformJson(json);
            return true;
        }

        public EventTransformations Register<TEvent>(string eventTypeName, Func<JsonDocument, TEvent> transformJson)
            where TEvent : notnull
        {
            jsonTransformations.Add(
                eventTypeName,
                json => transformJson(JsonDocument.Parse(json))
            );
            return this;
        }

        public EventTransformations Register<TOldEvent, TEvent>(string eventTypeName, Func<TOldEvent, TEvent> transformEvent)
            where TOldEvent : notnull
            where TEvent : notnull
        {
            jsonTransformations.Add(
                eventTypeName,
                json => transformEvent(JsonSerializer.Deserialize<TOldEvent>(json)!)
            );
            return this;
        }
    }

    public class EventTypeMapping
    {
        private readonly Dictionary<string, Type> mappings = new ();

        public EventTypeMapping Register<TEvent>(params string[] typeNames)
        {
            var eventType = typeof(TEvent);

            foreach (var typeName in typeNames)
            {
                mappings.Add(typeName, eventType);
            }

            return this;
        }

        public Type Map(string eventType) => mappings[eventType];
    }

    public class EventSerializer
    {
        private readonly EventTypeMapping mapping;
        private readonly EventTransformations transformations;

        public EventSerializer(EventTypeMapping mapping, EventTransformations transformations)
        {
            this.mapping = mapping;
            this.transformations = transformations;
        }

        public object? Deserialize(string eventTypeName, string json) =>
            transformations.TryTransform(eventTypeName, json, out var transformed)
                ? transformed : JsonSerializer.Deserialize(json, mapping.Map(eventTypeName));
    }

    [Fact]
    public void UpcastObjects_Should_BeForwardCompatible()
    {
        // Given
        const string eventTypeV1Name = "shopping_cart_initialized_v1";
        const string eventTypeV2Name = "shopping_cart_initialized_v2";
        const string eventTypeV3Name = "shopping_cart_initialized_v3";

        var mapping = new EventTypeMapping()
            .Register<ShoppingCartInitializedWithStatus>(
                eventTypeV1Name,
                eventTypeV2Name,
                eventTypeV3Name
            );

        var transformations = new EventTransformations()
            .Register(eventTypeV1Name, UpcastV1)
            .Register<ShoppingCartInitialized, ShoppingCartInitializedWithStatus>(eventTypeV2Name, UpcastV2);

        var serializer = new EventSerializer(mapping, transformations);

        var eventV1 = new V1.ShoppingCartInitialized(
            Guid.NewGuid(),
            Guid.NewGuid()
        );
        var eventV2 = new ShoppingCartInitialized(
            Guid.NewGuid(),
            new Client(Guid.NewGuid(), "Oscar the Grouch" )
        );
        var eventV3 = new ShoppingCartInitializedWithStatus(
            Guid.NewGuid(),
            new Client(Guid.NewGuid(), "Big Bird"),
            ShoppingCartStatus.Pending
        );

        var events = new []
        {
            new { EventType = eventTypeV1Name, EventData = JsonSerializer.Serialize(eventV1) },
            new { EventType = eventTypeV2Name, EventData = JsonSerializer.Serialize(eventV2) },
            new { EventType = eventTypeV3Name, EventData = JsonSerializer.Serialize(eventV3) }
        };

        // When
        var deserializedEvents = events
            .Select(ev => serializer.Deserialize(ev.EventType, ev.EventData))
            .OfType<ShoppingCartInitializedWithStatus>()
            .ToList();

        deserializedEvents.Should().HaveCount(3);

        // Then
        deserializedEvents[0].ShoppingCartId.Should().Be(eventV1.ShoppingCartId);
        deserializedEvents[0].Client.Should().NotBeNull();
        deserializedEvents[0].Client.Id.Should().Be(eventV1.ClientId);
        deserializedEvents[0].Client.Name.Should().Be("Unknown");
        deserializedEvents[0].Status.Should().Be(ShoppingCartStatus.Initialized);

        deserializedEvents[1].ShoppingCartId.Should().Be(eventV2.ShoppingCartId);
        deserializedEvents[1].Client.Should().Be(eventV2.Client);
        deserializedEvents[1].Status.Should().Be(ShoppingCartStatus.Initialized);

        deserializedEvents[2].ShoppingCartId.Should().Be(eventV3.ShoppingCartId);
        deserializedEvents[2].Client.Should().Be(eventV3.Client);
        deserializedEvents[2].Status.Should().Be(eventV3.Status);
    }
}