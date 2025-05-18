using System.Collections.Concurrent;

namespace BusinessProcesses.Core;

public class CommandBus
{
    public async ValueTask Send(object[] commands, CancellationToken ct)
    {
        foreach (var command in commands)
        {
            if (!commandHandlers.TryGetValue(command.GetType(), out var handler))
                continue;

            foreach (var middleware in middlewares)
                middleware(command);

            await handler(command, ct);
        }
    }

    public void Handle<T>(Func<T, CancellationToken, ValueTask> eventHandler)
    {
        commandHandlers[typeof(T)] = (command, ct) => eventHandler((T)command, ct);
    }

    public void Use(Action<object> middleware) =>
        middlewares.Add(middleware);

    private readonly ConcurrentDictionary<Type, Func<object, CancellationToken, ValueTask>> commandHandlers = new();
    private readonly List<Action<object>> middlewares = [];
}
