using Core.Events;
using Core.EventStoreDB.Events;
using Core.Tracing;
using Core.Tracing.Causation;
using Core.Tracing.Correlation;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Core.EventStoreDB.Tests.Events;

public class EventStoreDBEventMetadataJsonConverterTests
{
    private readonly EventStoreDBEventMetadataJsonConverter jsonConverter = new();

    [Fact]
    public void Serialize_ForNonEmptyEventMetadata_ShouldSucceed()
    {
        // Given
        var correlationId = new GuidCorrelationIdFactory().New();
        var causationId = new GuidCausationIdFactory().New();

        var eventMetadata = new TraceMetadata(correlationId, causationId);

        // When
        var json = JsonConvert.SerializeObject(eventMetadata, jsonConverter);


        json.Should().Be($"{{\"$correlationId\":\"{correlationId.Value}\",\"$causationId\":\"{causationId.Value}\"}}");
    }

    [Fact]
    public void DeSerialize_ForNonEmptyEventMetadata_ShouldSucceed()
    {
        // Given
        var correlationId = new GuidCorrelationIdFactory().New();
        var causationId = new GuidCausationIdFactory().New();
        var expectedEventMetadata = new TraceMetadata(correlationId, causationId);
        var json = $"{{\"$correlationId\":\"{correlationId.Value}\",\"$causationId\":\"{causationId.Value}\"}}";

        // When
        var eventMetadata = JsonConvert.DeserializeObject<TraceMetadata>(json, jsonConverter);


        eventMetadata.Should().Be(expectedEventMetadata);
    }
}
