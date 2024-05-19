using System.Text.Json;
using EventStore.Client;
using OptimisticConcurrency.Core.Entities;
using OptimisticConcurrency.Core.Exceptions;

namespace OptimisticConcurrency.Core.EventStoreDB;

public static class EventStoreDBExtensions
{
    public static Task<(T?, StreamRevision?)> AggregateStream<T, TEvent>(
        this EventStoreClient eventStore,
        Func<T> getInitial,
        Guid id,
        CancellationToken ct = default
    ) where T : Aggregate<TEvent> =>
        eventStore.AggregateStream<T, TEvent>(
            (state, @event) =>
            {
                state.Evolve(@event);
                return state;
            },
            getInitial,
            id,
            ct
        );

    public static async Task<(T?, StreamRevision?)> AggregateStream<T, TEvent>(
        this EventStoreClient eventStore,
        Func<T, TEvent, T> evolve,
        Func<T> getInitial,
        Guid id,
        CancellationToken cancellationToken = default
    ) where T : class
    {
        var result = eventStore.ReadStreamAsync(
            Direction.Forwards,
            ToStreamName<T>(id),
            StreamPosition.Start,
            cancellationToken: cancellationToken
        );

        if (await result.ReadState == ReadState.StreamNotFound)
            return (null, null);

        StreamRevision? revision = null;
        var state = await result
            .Select(@event =>
                {
                    revision = StreamRevision.FromStreamPosition(@event.Event.EventNumber);
                    return (TEvent)JsonSerializer.Deserialize(
                        @event.Event.Data.Span,
                        Type.GetType(@event.Event.EventType, true)!
                    )!;
                }
            )
            .AggregateAsync(
                getInitial(),
                evolve,
                cancellationToken
            );

        return (state, revision);
    }

    public static Task<StreamRevision> Add<T>(this EventStoreClient eventStore, Guid id, T aggregate,
        CancellationToken ct)
        where T : class, IAggregate =>
        eventStore.Add<T>(id, aggregate.DequeueUncommittedEvents(), ct);

    public static async Task<StreamRevision> Add<T>(this EventStoreClient eventStore, Guid id, object[] events,
        CancellationToken ct)
        where T : class
    {
        var result = await eventStore.AppendToStreamAsync(
            ToStreamName<T>(id),
            StreamState.NoStream,
            ToEventData(events),
            cancellationToken: ct
        );

        return result.NextExpectedStreamRevision;
    }

    public static Task<StreamRevision> GetAndUpdate<T, TEvent>(
        this EventStoreClient eventStore,
        Func<T> getInitial,
        Guid id,
        StreamRevision expectedRevision,
        Action<T> handle,
        CancellationToken ct
    )
        where T : Aggregate<TEvent>
        where TEvent : notnull =>
        eventStore.GetAndUpdate<T, TEvent>(
            (state, @event) =>
            {
                state.Evolve(@event);
                return state;
            },
            getInitial,
            id,
            expectedRevision,
            state =>
            {
                handle(state);
                var events = state.DequeueUncommittedEvents();
                return events;
            }, ct);

    public static Task<StreamRevision> GetAndUpdate<T, TEvent>(
        this EventStoreClient eventStore,
        Func<T, TEvent, T> evolve,
        Func<T> getInitial,
        Guid id,
        Func<T, TEvent[]> handle,
        CancellationToken ct
    )
        where T : class
        where TEvent : notnull =>
        eventStore.GetAndUpdate(evolve, getInitial, id, null, handle, ct);

    public static async Task<StreamRevision> GetAndUpdate<T, TEvent>(
        this EventStoreClient eventStore,
        Func<T, TEvent, T> evolve,
        Func<T> getInitial,
        Guid id,
        StreamRevision? expectedRevision,
        Func<T, TEvent[]> handle,
        CancellationToken ct
    )
        where T : class
        where TEvent : notnull
    {
        var streamName = ToStreamName<T>(id);
        var (current, actualRevision) = await eventStore.AggregateStream(evolve, getInitial, id, ct);

        var events = handle(current ?? throw NotFoundException.For<T>(id));

        var result = await eventStore.AppendToStreamAsync(
            streamName,
            expectedRevision ?? actualRevision ?? StreamRevision.None,
            ToEventData(events.Cast<object>()),
            cancellationToken: ct
        );

        return result.NextExpectedStreamRevision;
    }

    private static IEnumerable<EventData> ToEventData(IEnumerable<object> events) =>
        events.Select(@event =>
            new EventData(
                Uuid.NewUuid(),
                @event.GetType().FullName!,
                JsonSerializer.SerializeToUtf8Bytes(@event)
            )
        );

    private static string ToStreamName<T>(Guid id) =>
        $"{typeof(T).Name}-{id}";
}
