using Core.EventStoreDB.Serialization;
using Core.Exceptions;
using Core.OpenTelemetry;
using Core.Reflection;
using EventStore.Client;

namespace Core.EventStoreDB.Events;

public static class EventStoreDBExtensions
{
    public static async Task<TEntity?> Find<TEntity>(
        this EventStoreClient eventStore,
        Func<TEntity, object, TEntity> when,
        string id,
        CancellationToken cancellationToken
    ) where TEntity: class
    {
        var readResult = eventStore.ReadStreamAsync(
            Direction.Forwards,
            id,
            StreamPosition.Start,
            cancellationToken: cancellationToken
        );

        if (await readResult.ReadState.ConfigureAwait(false) == ReadState.StreamNotFound)
            return null;

        var result = ObjectFactory<TEntity>.GetDefaultOrUninitialized();
        await foreach (var @event in readResult)
            result = when(result, @event);
        return result;
    }

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

        var result = getDefault();
        await foreach (var @event in readResult)
            result = when(result, @event);
        return result;
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
            return [];

        var result =  new List<object>();
        await foreach (var @event in readResult)
            result.Add(@event);
        return result;
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

    public static async Task<TEvent?> ReadLastEvent<TEvent>(
        this EventStoreClient eventStore,
        string id,
        CancellationToken ct
    ) where TEvent : class
    {
        var resolvedEvent = await eventStore.ReadLastEvent(id, ct).ConfigureAwait(false);

        return resolvedEvent?.Deserialize<TEvent>();
    }

    public static async Task<ResolvedEvent?> ReadLastEvent(
        this EventStoreClient eventStore,
        string id,
        CancellationToken ct
    )
    {
        var result = eventStore.ReadStreamAsync(
            Direction.Backwards,
            id,
            StreamPosition.End,
            maxCount: 1,
            cancellationToken: ct
        );

        if (await result.ReadState.ConfigureAwait(false) == ReadState.StreamNotFound)
        {
            return null;
        }

        await foreach (var @event in result)
            return @event;

        return null;
    }

    public static async Task AppendToStreamWithSingleEvent(
        this EventStoreClient eventStore,
        string id,
        object @event,
        CancellationToken ct
    )
    {
        var eventData = new[] { @event.ToJsonEventData() };

        var result = await eventStore.AppendToStreamAsync(
            id,
            StreamState.StreamExists,
            eventData,
            options =>
            {
                options.ThrowOnAppendFailure = false;
            },
            cancellationToken: ct
        ).ConfigureAwait(false);

        if (result is SuccessResult)
            return;

        await eventStore.SetStreamMetadataAsync(
            id,
            StreamState.NoStream,
            new StreamMetadata(maxCount: 1),
            cancellationToken: ct
        ).ConfigureAwait(false);

        await eventStore.AppendToStreamAsync(
            id,
            StreamState.NoStream,
            eventData,
            cancellationToken: ct
        ).ConfigureAwait(false);
    }
}
