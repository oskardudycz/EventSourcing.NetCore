using System.Diagnostics;
using Core.EventStoreDB.Events;
using Core.OpenTelemetry;
using FluentAssertions;
using Newtonsoft.Json;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Xunit;

namespace Core.EventStoreDB.Tests.Events;

public class EventStoreDBEventMetadataJsonConverterTests
{
    private readonly EventStoreDBEventMetadataJsonConverter jsonConverter = new();
    private readonly ActivityScope activityScope = new();

    public EventStoreDBEventMetadataJsonConverterTests()
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        ActivitySourceProvider.AddDummyListener();
    }

    [Fact]
    public void Serialize_ForNonEmptyEventMetadata_ShouldSucceed()
    {
        // Given
        using var activity = activityScope.Start("test", new StartActivityOptions())!;

        var propagationContext = new PropagationContext(activity.Context, Baggage.Current);

        // When
        var json = JsonConvert.SerializeObject(propagationContext, jsonConverter);

        json.Should()
            .Be(
                $"{{\"$correlationId\":\"{activity.Context.TraceId}\",\"$causationId\":\"{activity.Context.SpanId}\"}}");
    }

    [Fact]
    public void DeSerialize_ForNonEmptyEventMetadata_ShouldSucceed()
    {
        // // Given
        // var correlationId = new GuidCorrelationIdFactory().New();
        // var causationId = new GuidCausationIdFactory().New();
        // var expectedEventMetadata = new PropagationContext(correlationId, causationId);
        // var json = $"{{\"$correlationId\":\"{correlationId.Value}\",\"$causationId\":\"{causationId.Value}\"}}";
        //
        // // When
        // var eventMetadata = JsonConvert.DeserializeObject<PropagationContext>(json, jsonConverter);
        //
        //
        // eventMetadata.Should().Be(expectedEventMetadata);
    }
}
