using System;
using System.Collections.Generic;
using System.Linq;
using V1 = ECommerce.V1;
using Xunit;
using System.Text.Json;
using FluentAssertions;

namespace EventsVersioning.Tests.Transformations;

public class MergeEvents
{
    public record ShoppingCartInitializedWithProducts(
        Guid ShoppingCartId,
        Guid ClientId,
        List<V1.PricedProductItem> ProductItems
    );

    public record EventMetadata(
        Guid CorrelationId
    );

    public record EventData(
        string EventType,
        string Data,
        string MetaData
    );

    public List<EventData> FlattenInitializedEventsWithProductItemsAdded(
        List<EventData> events
    )
    {
        var cartInitialized = events.First();
        var cartInitializedCorrelationId =
            JsonSerializer.Deserialize<EventMetadata>(cartInitialized.MetaData)!
                .CorrelationId;

        var i = 1;
        List<EventData> productItemsAdded = new();

        while (i < events.Count)
        {
            var eventData = events[i];

            if (eventData.EventType != "product_item_added_v1")
                break;

            var correlationId = JsonSerializer
                .Deserialize<EventMetadata>(eventData.MetaData)!
                .CorrelationId;

            if (correlationId != cartInitializedCorrelationId)
                break;

            productItemsAdded.Add(eventData);
            i++;
        }

        var mergedEvent = ToShoppingCartInitializedWithProducts(
            cartInitialized,
            productItemsAdded
        );

        return new List<EventData>(
            new[] { mergedEvent }.Union(events.Skip(i))
        );
    }

    private EventData ToShoppingCartInitializedWithProducts(
        EventData shoppingCartInitialized,
        List<EventData> productItemsAdded
    )
    {
        var shoppingCartInitializedJson = JsonDocument.Parse(shoppingCartInitialized!.Data).RootElement;

        var newEvent = new ShoppingCartInitializedWithProducts(
            shoppingCartInitializedJson.GetProperty("ShoppingCartId").GetGuid(),
            shoppingCartInitializedJson.GetProperty("ClientId").GetGuid(), new List<V1.PricedProductItem>(
                productItemsAdded.Select(pi =>
                {
                    var pricedProductItem = JsonDocument.Parse(pi.Data).RootElement.GetProperty("ProductItem");
                    var productItem = pricedProductItem.GetProperty("ProductItem");

                    return new V1.PricedProductItem(
                        new V1.ProductItem(productItem.GetProperty("ProductId").GetGuid(),
                            productItem.GetProperty("Quantity").GetInt32()),
                        pricedProductItem.GetProperty("UnitPrice").GetDecimal());
                })
            )
        );

        return new EventData("shopping_cart_initialized_v2", JsonSerializer.Serialize(newEvent),
            shoppingCartInitialized.MetaData);
    }

    public class StreamTransformations
    {
        private readonly List<Func<List<EventData>, List<EventData>>> jsonTransformations = new();

        public List<EventData> Transform(List<EventData> events)
        {
            if (!jsonTransformations.Any())
                return events;

            var result = jsonTransformations
                .Aggregate(events, (current, transform) => transform(current));

            return result;
        }

        public StreamTransformations Register(
            Func<List<EventData>, List<EventData>> transformJson
        )
        {
            jsonTransformations.Add(transformJson);
            return this;
        }
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

        public EventTransformations Register<TOldEvent, TEvent>(string eventTypeName,
            Func<TOldEvent, TEvent> transformEvent)
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
        private readonly Dictionary<string, Type> mappings = new();

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
        private readonly StreamTransformations streamTransformations;
        private readonly EventTransformations transformations;

        public EventSerializer(EventTypeMapping mapping, StreamTransformations streamTransformations,
            EventTransformations transformations)
        {
            this.mapping = mapping;
            this.transformations = transformations;
            this.streamTransformations = streamTransformations;
        }

        public object? Deserialize(string eventTypeName, string json) =>
            transformations.TryTransform(eventTypeName, json, out var transformed)
                ? transformed
                : JsonSerializer.Deserialize(json, mapping.Map(eventTypeName));

        public List<object?> Deserialize(List<EventData> events) =>
            streamTransformations.Transform(events)
                .Select(@event => Deserialize(@event.EventType, @event.Data))
                .ToList();
    }

    [Fact]
    public void UpcastObjects_Should_BeForwardCompatible()
    {
        // Given
        var mapping = new EventTypeMapping()
            .Register<ShoppingCartInitializedWithProducts>(
                "shopping_cart_initialized_v2"
            )
            .Register<V1.ProductItemAddedToShoppingCart>(
                "product_item_added_v1"
            )
            .Register<V1.ShoppingCartConfirmed>(
                "shopping_card_confirmed_v1"
            );

        var streamTransformations =
            new StreamTransformations()
                .Register(FlattenInitializedEventsWithProductItemsAdded);

        var serializer = new EventSerializer(
            mapping,
            streamTransformations,
            new EventTransformations()
        );

        var shoppingCardId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var theSameCorrelationId = Guid.NewGuid();
        var productItem = new V1.PricedProductItem(new V1.ProductItem(Guid.NewGuid(), 1), 23.22m);

        var events = new (string EventTypeName, object EventData, EventMetadata MetaData)[]
        {
            (
                "shopping_cart_initialized_v1",
                new V1.ShoppingCartInitialized(shoppingCardId, clientId),
                new EventMetadata(theSameCorrelationId)
            ),
            (
                "product_item_added_v1",
                new V1.ProductItemAddedToShoppingCart(shoppingCardId, productItem),
                new EventMetadata(theSameCorrelationId)
            ),
            (
                "product_item_added_v1",
                new V1.ProductItemAddedToShoppingCart(shoppingCardId, productItem),
                new EventMetadata(theSameCorrelationId)
            ),
            (
                "product_item_added_v1",
                new V1.ProductItemAddedToShoppingCart(shoppingCardId, productItem),
                new EventMetadata(Guid.NewGuid())
            ),
            (
                "shopping_card_confirmed_v1",
                new V1.ShoppingCartConfirmed(shoppingCardId, DateTime.UtcNow),
                new EventMetadata(Guid.NewGuid())
            )
        };

        var serialisedEvents = events.Select(e =>
            new EventData(
                e.EventTypeName,
                JsonSerializer.Serialize(e.EventData),
                JsonSerializer.Serialize(e.MetaData)
            )
        ).ToList();

        // When
        var deserializedEvents = serializer.Deserialize(serialisedEvents);

        // Then
        deserializedEvents.Should().HaveCount(3);
        deserializedEvents[0].As<ShoppingCartInitializedWithProducts>()
            .ClientId.Should().Be(clientId);
        deserializedEvents[0].As<ShoppingCartInitializedWithProducts>()
            .ShoppingCartId.Should().Be(shoppingCardId);
        deserializedEvents[0].As<ShoppingCartInitializedWithProducts>()
            .ProductItems.Should().HaveCount(2);
        deserializedEvents[0].As<ShoppingCartInitializedWithProducts>()
            .ProductItems.Should().OnlyContain(pr => pr.Equals(productItem));

        deserializedEvents[1].As<V1.ProductItemAddedToShoppingCart>()
            .ShoppingCartId.Should().Be(shoppingCardId);
        deserializedEvents[1].As<V1.ProductItemAddedToShoppingCart>()
            .ProductItem.Should().Be(productItem);

        deserializedEvents[2].As<V1.ShoppingCartConfirmed>()
            .ShoppingCartId.Should().Be(shoppingCardId);
    }
}
