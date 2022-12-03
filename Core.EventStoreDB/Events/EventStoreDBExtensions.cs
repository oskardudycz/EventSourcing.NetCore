using Core.EventStoreDB.Serialization;
using Core.Exceptions;
using Core.OpenTelemetry;
using EventStore.Client;

namespace Core.EventStoreDB.Events;

public static class EventStoreDBExtensions
{
    public static async Task<TEntity> Find<TEntity>(
        this EventStoreClient eventStore,
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

        if (await readResult.ReadState.ConfigureAwait(false) == ReadState.StreamNotFound)
            throw AggregateNotFoundException.For<TEntity>(id);

        return await readResult
            .Select(@event => @event.Deserialize()!)
            .AggregateAsync(
                getDefault(),
                when,
                cancellationToken
            ).ConfigureAwait(false);
    }

    public static async Task<List<object>> ReadStream(
        this EventStoreClient eventStore,
        string id,
        CancellationToken cancellationToken)
    {
        var readResult = eventStore.ReadStreamAsync(
            Direction.Forwards,
            id,
            StreamPosition.Start,
            cancellationToken: cancellationToken
        );

        if (await readResult.ReadState.ConfigureAwait(false) == ReadState.StreamNotFound)
            return new List<object>();

        return await readResult
            .Select(@event => @event.Deserialize()!)
            .ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public static async Task<ulong> Append(
        this EventStoreClient eventStore,
        string id,
        object @event,
        CancellationToken cancellationToken
    )

    {
        var result = await eventStore.AppendToStreamAsync(
            id,
            StreamState.NoStream,
            new[] { @event.ToJsonEventData(TelemetryPropagator.GetPropagationContext()) },
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        return result.NextExpectedStreamRevision;
    }


    public static async Task<ulong> Append(
        this EventStoreClient eventStore,
        string id,
        object @event,
        ulong expectedRevision,
        CancellationToken cancellationToken
    )
    {
        var result = await eventStore.AppendToStreamAsync(
            id,
            expectedRevision,
            new[] { @event.ToJsonEventData(TelemetryPropagator.GetPropagationContext()) },
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        return result.NextExpectedStreamRevision;
    }
}
