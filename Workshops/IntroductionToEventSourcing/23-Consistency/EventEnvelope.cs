namespace BusinessProcesses.Core;

public record EventMetadata(
    string EventId,
    ulong StreamPosition,
    ulong LogPosition
)
{
    public static EventMetadata For(ulong streamPosition, ulong logPosition) =>
        new(Guid.NewGuid().ToString(), streamPosition, logPosition);
}

public record EventEnvelope(
    object Data,
    EventMetadata Metadata
);

public record EventEnvelope<T>(
    T Data,
    EventMetadata Metadata
): EventEnvelope(Data, Metadata) where T : notnull
{
    public new T Data => (T)base.Data;
}
