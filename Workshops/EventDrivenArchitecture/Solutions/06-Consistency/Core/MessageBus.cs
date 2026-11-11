using System.Collections.Concurrent;

namespace Consistency.Core;

/// <summary>
/// Delivers messages from publishers to subscribers.
/// Commands and events both travel through this bus. The difference is intent, not transport:
/// <list type="bullet">
/// <item>a command asks for something to be done, e.g. <c>RecordGuestCheckoutCompletion</c>.
/// It has one intended recipient, which may accept or reject it;</item>
/// <item>an event records something that already happened, e.g. <c>GuestCheckedOut</c>.
/// Any number of subscribers may react to it, or none.</item>
/// </list>
/// The bus delivers each message to every subscriber registered for its type. It doesn't check
/// that a command has exactly one subscriber; that's up to how the application wires it.
/// </summary>
public class MessageBus
{
    public async ValueTask Publish(object[] messages, CancellationToken ct)
    {
        failures.FailIfUnavailable();

        foreach (var message in messages)
        {
            foreach (var middleware in middlewares)
                middleware(message);

            if (!messageHandlers.TryGetValue(message.GetType(), out var handlers))
                continue;

            foreach (var handler in handlers)
                await handler(message, ct);
        }
    }

    public MessageBus Subscribe<T>(Func<T, CancellationToken, ValueTask> messageHandler)
    {
        Func<object, CancellationToken, ValueTask> handler = (message, ct) => messageHandler((T)message, ct);

        messageHandlers.AddOrUpdate(
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

    private readonly ConcurrentDictionary<Type, List<Func<object, CancellationToken, ValueTask>>> messageHandlers = new();
    private readonly List<Action<object>> middlewares = [];
    private readonly TransientFailures failures = new("Message bus not available!");
}
