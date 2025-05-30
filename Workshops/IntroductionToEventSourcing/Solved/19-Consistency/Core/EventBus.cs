using System.Collections.Concurrent;

namespace Consistency.Core;

public class EventStore
{
    public async ValueTask AppendToStream(object[] events, CancellationToken ct)
    {
        if (DateTimeOffset.Now.Ticks % 5 == 0)
            throw new TimeoutException("Database not available!");

        foreach (var @event in events)
        {
            foreach (var middleware in middlewares)
                middleware(@event);

            if (!eventHandlers.TryGetValue(@event.GetType(), out var handlers))
                continue;

            foreach (var handler in handlers)
                await handler(@event, ct);
        }
    }

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

    private readonly ConcurrentDictionary<Type, List<Func<object, CancellationToken, ValueTask>>> eventHandlers = new();
    private readonly List<Action<object>> middlewares = [];
}
