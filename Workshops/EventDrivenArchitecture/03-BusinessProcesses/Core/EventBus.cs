using System.Collections.Concurrent;

namespace BusinessProcesses.Core;

public class EventBus
{
    public ValueTask Publish(object[] events, CancellationToken _)
    {
        foreach (var @event in events)
        {
            if (!eventHandlers.TryGetValue(@event.GetType(), out var handlers))
                continue;

            foreach (var middleware in middlewares)
                middleware(@event);

            foreach (var handler in handlers)
                handler(@event);
        }

        return ValueTask.CompletedTask;
    }

    public void Subscribe<T>(Action<T> eventHandler)
    {
        Action<object> handler = x => eventHandler((T)x);

        eventHandlers.AddOrUpdate(
            typeof(T),
            _ => [handler],
            (_, handlers) =>
            {
                handlers.Add(handler);
                return handlers;
            }
        );
    }

    public void Use(Action<object> middleware) =>
        middlewares.Add(middleware);

    private readonly ConcurrentDictionary<Type, List<Action<object>>> eventHandlers = new();
    private readonly List<Action<object>> middlewares = [];
}
