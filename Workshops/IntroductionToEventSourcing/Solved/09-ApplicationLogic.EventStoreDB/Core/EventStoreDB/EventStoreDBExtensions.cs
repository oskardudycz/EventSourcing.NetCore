using System.Text.Json;
using ApplicationLogic.EventStoreDB.Core.Entities;
using ApplicationLogic.EventStoreDB.Core.Exceptions;
using EventStore.Client;

namespace ApplicationLogic.EventStoreDB.Core.EventStoreDB;

public static class EventStoreDBExtensions
{
    public static Task<T?> AggregateStream<T, TEvent>(
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

    public static async Task<T?> AggregateStream<T, TEvent>(
        this EventStoreClient eventStore,
        Func<T, TEvent, T> evolve,
        Func<T> getInitial,
        Guid id,
        CancellationToken cancellationToken = default
    ) where T : class
    {
        var readResult = eventStore.ReadStreamAsync(
            Direction.Forwards,
            ToStreamName<T>(id),
            StreamPosition.Start,
            cancellationToken: cancellationToken
        );

        if (await readResult.ReadState == ReadState.StreamNotFound)
            return null;

        var result = getInitial();
        await foreach (var @event in readResult)
            result = evolve(result, (TEvent)JsonSerializer.Deserialize(
                @event.Event.Data.Span,
                Type.GetType(@event.Event.EventType, true)!
            )!);
        return result;
    }

    public static Task Add<T>(this EventStoreClient eventStore, Guid id, T aggregate, CancellationToken ct)
        where T : class, IAggregate =>
        eventStore.Add<T>(id, aggregate.DequeueUncommittedEvents(), ct);

    public static Task Add<T>(this EventStoreClient eventStore, Guid id, object[] events, CancellationToken ct)
        where T : class =>
        eventStore.AppendToStreamAsync(
            ToStreamName<T>(id),
            StreamState.NoStream,
            ToEventData(events),
            cancellationToken: ct
        );

    public static Task GetAndUpdate<T, TEvent>(
        this EventStoreClient eventStore,
        Func<T> getInitial,
        Guid id,
        Action<T> handle,
        CancellationToken ct
    )
        where T : Aggregate<TEvent>
        where TEvent: notnull =>
        eventStore.GetAndUpdate<T, TEvent>(
            (state, @event) =>
            {
                state.Evolve(@event);
                return state;
            },
            getInitial,
            id,
            state =>
            {
                handle(state);
                var events = state.DequeueUncommittedEvents();
                return events;
            }, ct);

    public static async Task GetAndUpdate<T, TEvent>(
        this EventStoreClient eventStore,
        Func<T, TEvent, T> evolve,
        Func<T> getInitial,
        Guid id,
        Func<T, TEvent[]> handle,
        CancellationToken ct
    )
        where T : class
        where TEvent: notnull
    {
        var streamName = ToStreamName<T>(id);
        var current = await eventStore.AggregateStream(evolve, getInitial, id, ct);

        var events = handle(current ?? throw NotFoundException.For<T>(id));

        await eventStore.AppendToStreamAsync(
            streamName,
            StreamState.Any,
            ToEventData(events.Cast<object>()),
            cancellationToken: ct
        );
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
