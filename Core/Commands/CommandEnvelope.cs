using System.Reflection;
using Core.Tracing;

namespace Core.Commands;

public record CommandMetadata(
    string CommandId,
    string Target,
    ulong? ExpectedVersion,
    TraceMetadata? Trace
);

public interface ICommandEnvelope
{
    object Data { get; }
    CommandMetadata Metadata { get; init; }
}

public record CommandEnvelope<T>(
    T Data,
    CommandMetadata Metadata
): ICommandEnvelope where T : notnull
{
    object ICommandEnvelope.Data => Data;
}

public static class CommandEnvelopeFactory
{
    public static ICommandEnvelope From(object data, CommandMetadata metadata)
    {
        //TODO: Get rid of reflection!
        var type = typeof(CommandEnvelope<>).MakeGenericType(data.GetType());
        return (ICommandEnvelope)Activator.CreateInstance(type, data, metadata)!;
    }
}
