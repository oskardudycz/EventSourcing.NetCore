using Core.Structures;
using Marten;

namespace Core.Marten.Extensions;

public static class DocumentSessionExtensions
{
    public static Task Add<T>(
        this IDocumentSession documentSession,
        Guid id,
        object @event,
        CancellationToken ct
    ) where T : class
    {
        documentSession.Events.StartStream<T>(id, @event);
        return documentSession.SaveChangesAsync(token: ct);
    }

    public static Task Add<T>(
        this IDocumentSession documentSession,
        string id,
        object @event,
        CancellationToken ct
    ) where T : class
    {
        documentSession.Events.StartStream<T>(id, @event);
        return documentSession.SaveChangesAsync(token: ct);
    }

    public static Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        Guid id, int version,
        Func<T, object> handle,
        Func<T> initial,
        CancellationToken ct
    ) where T : class =>
        documentSession.Events.WriteToAggregate<T>(id, version, stream =>
            stream.AppendOne(handle(stream.Aggregate ?? initial())), ct);

    public static Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        string id,
        int version,
        Func<T, IEnumerable<EventOrCommand>> handle,
        Func<T> initial,
        CancellationToken ct
    ) where T : class =>
        documentSession.Events.WriteToAggregate<T>(id, version, stream =>
        {
            var messages = handle(stream.Aggregate ?? initial());

            foreach (var message in messages)
            {
                message.Switch(
                    stream.AppendOne,
                    command => documentSession.Events.Append($"commands-{id}", command)
                );
            }
        }, ct);


    public static Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        string id,
        Func<T, IEnumerable<EventOrCommand>> handle,
        Func<T> initial,
        CancellationToken ct
    ) where T : class =>
        documentSession.Events.WriteToAggregate<T>(id, stream =>
        {
            var messages = handle(stream.Aggregate ?? initial());

            foreach (var message in messages)
            {
                message.Switch(
                    stream.AppendOne,
                    command => documentSession.Events.Append($"commands-{id}", command)
                );
            }
        }, ct);

    public static Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        string id,
        Func<T, IEnumerable<object>> handle,
        Func<T> initial,
        CancellationToken ct
    ) where T : class =>
        documentSession.Events.WriteToAggregate<T>(id,
            stream => stream.AppendMany(handle(stream.Aggregate ?? initial())), ct);

    public static Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        Guid id,
        Func<T, object> handle,
        Func<T> initial,
        CancellationToken ct
    ) where T : class =>
        documentSession.Events.WriteToAggregate<T>(id, stream =>
            stream.AppendOne(handle(stream.Aggregate ?? initial())), ct);

    public static Task GetAndUpdate<T, TEvent>(
        this IDocumentSession documentSession,
        Guid id,
        Func<T, Maybe<TEvent>> handle,
        Func<T> initial,
        CancellationToken ct
    ) where T : class =>
        documentSession.Events.WriteToAggregate<T>(id, stream =>
            handle(stream.Aggregate ?? initial()).IfExists(@event => stream.AppendOne(@event!)), ct);

    public static Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        Guid id,
        Func<T, Maybe<object>> handle,
        Func<T> initial,
        CancellationToken ct
    ) where T : class =>
        documentSession.Events.WriteToAggregate<T>(id, stream =>
            handle(stream.Aggregate ?? initial()).IfExists(stream.AppendOne), ct);

    public static Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        Guid id,
        Func<T, Maybe<object[]>> handle,
        Func<T> initial,
        CancellationToken ct
    ) where T : class =>
        documentSession.Events.WriteToAggregate<T>(id, stream =>
            handle(stream.Aggregate ?? initial()).IfExists(stream.AppendMany), ct);
}
