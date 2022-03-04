using Core.Events;
using Core.EventStoreDB.Serialization;
using EventStore.Client;

namespace ECommerce.Core.EventStoreDB;

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

        return (await readResult
            .Select(@event => @event.Deserialize()!)
            .AggregateAsync(
                getDefault(),
                when,
                cancellationToken
            ))!;
    }

    public static async Task<ulong> Append(
        this EventStoreClient eventStore,
        string id,
        object @event,
        EventMetadata eventMetadata,
        CancellationToken cancellationToken
    )

    {
        var result = await eventStore.AppendToStreamAsync(
            id,
            StreamState.NoStream,
            new[] { @event.ToJsonEventData(eventMetadata) },
            cancellationToken: cancellationToken
        );
        return result.NextExpectedStreamRevision;
    }


    public static async Task<ulong> Append(
        this EventStoreClient eventStore,
        string id,
        object @event,
        ulong expectedRevision,
        EventMetadata eventMetadata,
        CancellationToken cancellationToken
    )
    {
        var result = await eventStore.AppendToStreamAsync(
            id,
            expectedRevision,
            new[] { @event.ToJsonEventData(eventMetadata) },
            cancellationToken: cancellationToken
        );

        return result.NextExpectedStreamRevision;
    }
}
