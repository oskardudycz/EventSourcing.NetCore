using ApplicationLogic.Marten.Core.Entities;
using ApplicationLogic.Marten.Core.Exceptions;
using Marten;

namespace ApplicationLogic.Marten.Core.Marten;

public static class DocumentSessionExtensions
{
    public static Task Add<T>(this IDocumentSession documentSession, Guid id, T aggregate, CancellationToken ct)
        where T : class, IAggregate =>
        documentSession.Add<T>(id, aggregate.DequeueUncommittedEvents(), ct);

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
        documentSession.GetAndUpdate<T>(id, state =>
        {
            handle(state);
            var events = state.DequeueUncommittedEvents();
            return events;
        }, ct);
}
