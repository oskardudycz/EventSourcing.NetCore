using Core.Tracing;

namespace Core.Events;

public record EventMetadata(
    string EventId,
    ulong StreamPosition,
    ulong LogPosition,
    TraceMetadata? Trace
);

public record EventEnvelope(
    object Data,
    EventMetadata Metadata
): IEvent;

public record EventEnvelope<T>(
    T Data,
    EventMetadata Metadata
): EventEnvelope(Data, Metadata) where T : notnull
{
    public new T Data => (T)base.Data;
}
