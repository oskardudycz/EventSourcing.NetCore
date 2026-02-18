using Marten;

namespace PointOfSales.Api.Core.Marten;

public static class DocumentSessionExtensions
{
    public static Task Add<T>(this IDocumentSession documentSession, string id, object[] events, CancellationToken ct)
        where T : class =>
        documentSession.Add<T, object>(id, events, ct);

    public static Task Add<T, TEvent>(this IDocumentSession documentSession, string id, TEvent[] events,
        CancellationToken ct)
        where T : class
    {
        if (events.Length == 0)
            return Task.CompletedTask;

        documentSession.Events.StartStream<T>(id, events.Cast<object>().ToArray());
        return documentSession.SaveChangesAsync(token: ct);
    }

    public static Task GetAndUpdate<T, TEvent>(
        this IDocumentSession documentSession,
        string id,
        int version,
        Func<T, TEvent[]> handle,
        Func<T> initial,
        CancellationToken ct
    ) where T : class =>
        documentSession.Events.WriteToAggregate<T>(id, version, stream =>
        {
            var result = handle(stream.Aggregate ?? initial());
            if (result.Length != 0)
                stream.AppendMany(result.Cast<object>().ToArray());
        }, ct);
}
