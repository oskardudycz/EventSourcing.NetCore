using System.Collections.Concurrent;
using FluentAssertions;

namespace BusinessProcesses.Core;

public class CommandBus
{
    public ValueTask Send(object[] commands, CancellationToken _)
    {
        foreach (var command in commands)
        {
            if (!commandHandlers.TryGetValue(command.GetType(), out var handler))
                continue;

            foreach (var middleware in middlewares)
                middleware(command);

            handler(command);
        }

        return ValueTask.CompletedTask;
    }

    public void Handle<T>(Action<T> eventHandler)
    {
        commandHandlers[typeof(T)] = x => eventHandler((T)x);
    }

    public void Use(Action<object> middleware) =>
        middlewares.Add(middleware);

    private readonly ConcurrentDictionary<Type, Action<object>> commandHandlers = new();
    private readonly List<Action<object>> middlewares = [];
}
