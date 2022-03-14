using System.Text.Json;
using EventStore.Client;

namespace IntroductionToEventSourcing.OptimisticConcurrency.Immutable;

public static class EventStoreClientExtensions
{
    public static async Task<TEntity> Get<TEntity>(
        this EventStoreClient eventStore,
        Func<TEntity, object, TEntity> when,
        TEntity empty,
        string streamName,
        CancellationToken cancellationToken = default
    )
    {
        var result = eventStore.ReadStreamAsync(
            Direction.Forwards,
            streamName,
            StreamPosition.Start,
            cancellationToken: cancellationToken
        );

        if (await result.ReadState == ReadState.StreamNotFound)
            throw new InvalidOperationException("Shopping Cart was not found!");

        return await result
            .Select(@event =>
                JsonSerializer.Deserialize(
                    @event.Event.Data.Span,
                    Type.GetType(@event.Event.EventType)!
                )!
            )
            .AggregateAsync(
                empty,
                when,
                cancellationToken
            );
    }

    public static async Task<ulong> Add<TCommand>(
        this EventStoreClient eventStore,
        Func<TCommand, string> getStreamName,
        Func<TCommand, object> action,
        TCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var @event = action(command);

        var result = await eventStore.AppendToStreamAsync(
            getStreamName(command),
            StreamState.NoStream,
            new[]
            {
                new EventData(
                    Uuid.NewUuid(),
                    @event.GetType().FullName!,
                    JsonSerializer.SerializeToUtf8Bytes(@event)
                )
            },
            cancellationToken: cancellationToken
        );

        return result.NextExpectedStreamRevision;
    }

    public static async Task<ulong> GetAndUpdate<TEntity, TCommand>(
        this EventStoreClient eventStore,
        Func<TEntity, object, TEntity> when,
        TEntity empty,
        Func<TCommand, string> getStreamName,
        Func<TCommand, TEntity, object> action,
        TCommand command,
        ulong expectedRevision,
        CancellationToken cancellationToken = default
    )
    {
        var streamName = getStreamName(command);
        var current = await eventStore.Get(when, empty, streamName, cancellationToken);

        var @event = action(command, current);

        var result = await eventStore.AppendToStreamAsync(
            getStreamName(command),
            expectedRevision,
            new[]
            {
                new EventData(
                    Uuid.NewUuid(),
                    @event.GetType().FullName!,
                    JsonSerializer.SerializeToUtf8Bytes(@event)
                )
            },
            cancellationToken: cancellationToken
        );

        return result.NextExpectedStreamRevision;
    }
}
