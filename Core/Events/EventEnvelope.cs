using System.Reflection;
using Core.Tracing;

namespace Core.Events;

public record EventMetadata(
    string EventId,
    ulong StreamPosition,
    ulong LogPosition,
    TraceMetadata? Trace
);

public interface IEventEnvelope
{
    object Data { get; }
    EventMetadata Metadata { get; init; }
}

public record EventEnvelope<T>(
    T Data,
    EventMetadata Metadata
): IEventEnvelope where T : notnull
{
    object IEventEnvelope.Data => Data;
}

public static class EventEnvelopeFactory
{
    public static IEventEnvelope From(object data, EventMetadata metadata)
    {
        //TODO: Get rid of reflection!
        var type = typeof(EventEnvelope<>).MakeGenericType(data.GetType());
        return (IEventEnvelope)Activator.CreateInstance(type, data, metadata)!;
    }
}
