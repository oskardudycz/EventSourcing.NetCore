using Core.KurrentDB.Serialization;
using Core.Exceptions;
using Core.OpenTelemetry;
using KurrentDB.Client;

namespace ECommerce.Core.KurrentDB;

public static class KurrentDBExtensions
{
    public static async Task<TEntity> Find<TEntity>(
        this KurrentDBClient eventStore,
        Func<TEntity> getDefault,
        Func<TEntity, object, TEntity> when,
        string id,
        CancellationToken cancellationToken)
    {
        var readResult = eventStore.ReadStreamAsync(
            Direction.Forwards,
            id,
            StreamPosition.Start,
            cancellationToken: cancellationToken
        );

        if (await readResult.ReadState == ReadState.StreamNotFound)
            throw AggregateNotFoundException.For<TEntity>(id);

        var result = getDefault();
        await foreach (var @event in readResult)
            result = when(result, @event.Deserialize()!);
        return result;
    }

    public static async Task<List<object>> ReadStream(
        this KurrentDBClient eventStore,
        string id,
        CancellationToken cancellationToken)
    {
        var readResult = eventStore.ReadStreamAsync(
            Direction.Forwards,
            id,
            StreamPosition.Start,
            cancellationToken: cancellationToken
        );

        if (await readResult.ReadState == ReadState.StreamNotFound)
            return [];

        var result = new List<object>();
        await foreach (var @event in readResult)
            result.Add(@event.Deserialize()!);
        return result;
    }

    public static async Task<ulong> Append(
        this KurrentDBClient eventStore,
        string id,
        object @event,
        CancellationToken cancellationToken
    )

    {
        var result = await eventStore.AppendToStreamAsync(
            id,
            StreamState.NoStream,
            [@event.ToJsonEventData(TelemetryPropagator.GetPropagationContext())],
            cancellationToken: cancellationToken
        );
        return (ulong)result.NextExpectedStreamState.ToInt64();
    }


    public static async Task<ulong> Append(
        this KurrentDBClient eventStore,
        string id,
        object @event,
        ulong expectedRevision,
        CancellationToken cancellationToken
    )
    {
        var result = await eventStore.AppendToStreamAsync(
            id,
            expectedRevision,
            [@event.ToJsonEventData(TelemetryPropagator.GetPropagationContext())],
            cancellationToken: cancellationToken
        );

        return (ulong)result.NextExpectedStreamState.ToInt64();
    }
}
