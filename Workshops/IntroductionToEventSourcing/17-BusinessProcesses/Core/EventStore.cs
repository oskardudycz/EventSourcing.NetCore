using System.Collections.Concurrent;

namespace BusinessProcesses.Core;

public class EventStore
{
    public async ValueTask<T> AggregateStream<T, TEvent>(
        string id,
        Func<T, TEvent, T> evolve,
        Func<T> getInitial,
        CancellationToken ct = default
    ) where T : class? where TEvent : notnull =>
        (await ReadStream<TEvent>(id, ct)).Aggregate(getInitial(), evolve);

    public async ValueTask AppendToStream(string streamId, object[] toAppend, CancellationToken ct = default)
    {
        if (!events.ContainsKey(streamId))
            events[streamId] = [];

        foreach (var @event in toAppend)
        {
            var eventEnvelope = new EventEnvelope(@event,
                EventMetadata.For(
                    (ulong)events[streamId].Count + 1,
                    (ulong)events.Values.Sum(s => s.Count)
                )
            );

            events[streamId].Add(eventEnvelope);

            foreach (var middleware in middlewares)
                middleware(@event);

            if (!eventHandlers.TryGetValue(@event.GetType(), out var handlers))
                continue;

            foreach (var handler in handlers)
                await handler(@event, ct);
        }
    }

    public ValueTask<TEvent[]> ReadStream<TEvent>(string streamId, CancellationToken ct = default)
        where TEvent : notnull =>
        ValueTask.FromResult(events.TryGetValue(streamId, out var stream)
            ? stream.Select(e => e.Data).Cast<TEvent>().ToArray()
            : []);

    public EventStore Subscribe<T>(Func<T, CancellationToken, ValueTask> eventHandler)
    {
        Func<object, CancellationToken, ValueTask> handler = (@event, ct) => eventHandler((T)@event, ct);

        eventHandlers.AddOrUpdate(
            typeof(T),
            _ => [handler],
            (_, handlers) =>
            {
                handlers.Add(handler);
                return handlers;
            }
        );

        return this;
    }

    public void Use(Action<object> middleware) =>
        middlewares.Add(middleware);

    private readonly Dictionary<string, List<EventEnvelope>> events = new();
    private readonly ConcurrentDictionary<Type, List<Func<object, CancellationToken, ValueTask>>> eventHandlers = new();
    private readonly List<Action<object>> middlewares = [];
}
