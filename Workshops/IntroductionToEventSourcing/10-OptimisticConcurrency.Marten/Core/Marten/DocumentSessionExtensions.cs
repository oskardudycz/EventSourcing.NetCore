using Marten;
using OptimisticConcurrency.Core.Entities;
using OptimisticConcurrency.Core.Exceptions;

namespace OptimisticConcurrency.Core.Marten;

public static class DocumentSessionExtensions
{
    public static Task Add<T>(this IDocumentSession documentSession, Guid id, object @event, CancellationToken ct)
        where T : class
    {
        documentSession.Events.StartStream<T>(id, @event);
        return documentSession.SaveChangesAsync(token: ct);
    }

    public static Task Add<T>(this IDocumentSession documentSession, Guid id, object[] events, CancellationToken ct)
        where T : class
    {
        documentSession.Events.StartStream<T>(id, events);
        return documentSession.SaveChangesAsync(token: ct);
    }

    public static Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        Guid id,
        Func<T, object[]> handle,
        CancellationToken ct
    ) where T : class =>
        documentSession.Events.WriteToAggregate<T>(id, stream =>
            stream.AppendMany(handle(stream.Aggregate ?? throw NotFoundException.For<T>(id))), ct);

    public static Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        Guid id,
        Action<T> handle,
        CancellationToken ct
    ) where T : class, IAggregate =>
        documentSession.Events.WriteToAggregate<T>(id, stream =>
        {
            var aggregate = stream.Aggregate ?? throw NotFoundException.For<T>(id);
            handle(aggregate);
            stream.AppendMany(aggregate.DequeueUncommittedEvents());
        }, ct);
}
