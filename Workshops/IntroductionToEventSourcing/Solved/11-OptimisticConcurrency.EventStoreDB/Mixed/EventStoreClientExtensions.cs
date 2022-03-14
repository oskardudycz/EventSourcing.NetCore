using System.Text.Json;
using EventStore.Client;

namespace IntroductionToEventSourcing.OptimisticConcurrency.Mixed;

public static class EventStoreClientExtensions
{
    public static async Task<TAggregate> Get<TAggregate>(
        this EventStoreClient eventStore,
        string streamName,
        CancellationToken cancellationToken = default
    ) where TAggregate : IAggregate
    {
        var result = eventStore.ReadStreamAsync(
            Direction.Forwards,
            streamName,
            StreamPosition.Start,
            cancellationToken: cancellationToken
        );

        if (await result.ReadState == ReadState.StreamNotFound)
            throw new InvalidOperationException("Shopping Cart was not found!");

        var empty = (TAggregate)Activator.CreateInstance(typeof(TAggregate), true)!;

        return await result
            .Select(@event =>
                JsonSerializer.Deserialize(
                    @event.Event.Data.Span,
                    Type.GetType(@event.Event.EventType)!
                )!
            )
            .AggregateAsync(
                empty,
                ((aggregate, @event) =>
                {
                    aggregate.When(@event!);
                    return aggregate;
                }),
                cancellationToken
            );
    }

    public static async Task<ulong> Add<TAggregate, TCommand>(
        this EventStoreClient eventStore,
        Func<TCommand, string> getStreamName,
        Func<TCommand, object> action,
        TCommand command,
        CancellationToken cancellationToken = default
    ) where TAggregate : IAggregate
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

    public static async Task<ulong> GetAndUpdate<TAggregate, TCommand>(
        this EventStoreClient eventStore,
        Func<TCommand, string> getStreamName,
        Func<TCommand, TAggregate, object> action,
        TCommand command,
        ulong expectedRevision,
        CancellationToken cancellationToken = default
    ) where TAggregate : IAggregate
    {
        var streamName = getStreamName(command);
        var current = await eventStore.Get<TAggregate>(streamName, cancellationToken);

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
