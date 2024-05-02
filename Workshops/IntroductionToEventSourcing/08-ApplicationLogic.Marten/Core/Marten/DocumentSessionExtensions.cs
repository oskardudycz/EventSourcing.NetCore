using ApplicationLogic.Marten.Core.Entities;
using Marten;

namespace ApplicationLogic.Marten.Core.Marten;

public static class DocumentSessionExtensions
{
    public static Task Add<T>(this IDocumentSession documentSession, Guid id, T aggregate, CancellationToken ct)
        where T : class, IAggregate =>
        throw new NotImplementedException("Document Session Extensions not implemented!");

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
