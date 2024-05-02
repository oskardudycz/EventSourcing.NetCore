using ApplicationLogic.EventStoreDB.Core.Entities;
using Marten;

namespace ApplicationLogic.EventStoreDB.Core.Marten;

public static class DocumentSessionExtensions
{
    public static Task Add<T>(this IDocumentSession documentSession, Guid id, object @event, CancellationToken ct)
        where T : class
        => throw new NotImplementedException("Document Session Extensions not implemented!");

    public static Task Add<T>(this IDocumentSession documentSession, Guid id, object[] events, CancellationToken ct)
        where T : class
        => throw new NotImplementedException("Document Session Extensions not implemented!");

    public static Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        Guid id,
        Func<T, object[]> handle,
        CancellationToken ct
    ) where T : class => throw new NotImplementedException("Document Session Extensions not implemented!");

    public static Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        Guid id,
        Action<T> handle,
        CancellationToken ct
    ) where T : class, IAggregate => throw new NotImplementedException("Document Session Extensions not implemented!");
}
