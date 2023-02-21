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

    public static Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        Guid id, int version,
        Func<T, object> handle,
        CancellationToken ct
    ) where T : class =>
        documentSession.Events.WriteToAggregate<T>(id, version, stream =>
            stream.AppendOne(handle(stream.Aggregate)), ct);

    public static Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        Guid id,
        Func<T, object> handle,
        CancellationToken ct
    ) where T : class =>
        documentSession.Events.WriteToAggregate<T>(id, stream =>
            stream.AppendOne(handle(stream.Aggregate)), ct);

    public static Task GetAndUpdate<T, TEvent>(
        this IDocumentSession documentSession,
        Guid id,
        Func<T, Maybe<TEvent>> handle,
        CancellationToken ct
    ) where T : class =>
        documentSession.Events.WriteToAggregate<T>(id, stream =>
            handle(stream.Aggregate).IfExists(@event => stream.AppendOne(@event)), ct);

    public static Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        Guid id,
        Func<T, Maybe<object>> handle,
        CancellationToken ct
    ) where T : class =>
        documentSession.Events.WriteToAggregate<T>(id, stream =>
            handle(stream.Aggregate).IfExists(stream.AppendOne), ct);

    public static Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        Guid id,
        Func<T, Maybe<object[]>> handle,
        CancellationToken ct
    ) where T : class =>
        documentSession.Events.WriteToAggregate<T>(id, stream =>
            handle(stream.Aggregate).IfExists(stream.AppendMany), ct);
}
