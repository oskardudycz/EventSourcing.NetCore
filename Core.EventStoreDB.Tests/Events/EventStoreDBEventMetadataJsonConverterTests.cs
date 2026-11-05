using System.Diagnostics;
using Core.EventStoreDB.Events;
using Core.OpenTelemetry;
using FluentAssertions;
using System.Text.Json;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Xunit;

namespace Core.EventStoreDB.Tests.Events;

public class EventStoreDBEventMetadataJsonConverterTests
{
    private readonly JsonSerializerOptions serializerOptions = new()
    {
        Converters = { new EventStoreDBEventMetadataJsonConverter() }
    };
    private readonly ActivityScope activityScope = new();

    public EventStoreDBEventMetadataJsonConverterTests()
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        ActivitySourceProvider.AddDummyListener();
        TelemetryPropagator.UseDefaultCompositeTextMapPropagator();
    }

    [Fact]
    public void Serialize_ForNonEmptyEventMetadata_ShouldSucceed()
    {
        // Given
        using var activity = activityScope.Start("test", new StartActivityOptions())!;

        var propagationContext = new PropagationContext(activity.Context, Baggage.Current);

        // When
        var json = JsonSerializer.Serialize(propagationContext, serializerOptions);

        json.Should()
            .Be(
                $"{{\"$correlationId\":\"{activity.Context.TraceId}\",\"$causationId\":\"{activity.Context.SpanId}\",\"traceparent\":\"00-{propagationContext.ActivityContext.TraceId}-{propagationContext.ActivityContext.SpanId}-01\"}}");
    }

    [Fact]
    public void Deserialize_ForNonEmptyEventMetadata_ShouldSucceed()
    {
        // Given
        const string traceId = "83c414374747d7383946dcf56830cb79";
        const string spanId = "0c180e05010f2d3f";
        var expectedPropagationContext = new PropagationContext(
            new ActivityContext(
                ActivityTraceId.CreateFromString(traceId),
                ActivitySpanId.CreateFromString(spanId),
                ActivityTraceFlags.Recorded,
                isRemote: true
            ),
            Baggage.Create()
        );
        var json =
            $"{{\"$correlationId\":\"{traceId}\",\"$causationId\":\"{spanId}\",\"traceparent\":\"00-{traceId}-{spanId}-01\"}}";

        // When
        var propagationContext = JsonSerializer.Deserialize<PropagationContext>(json, serializerOptions);

        propagationContext.Should().Be(expectedPropagationContext);
    }
}
