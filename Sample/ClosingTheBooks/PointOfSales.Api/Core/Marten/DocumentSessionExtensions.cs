using Marten;

namespace Helpdesk.Api.Core.Marten;

public static class DocumentSessionExtensions
{
    public static Task Add<T>(this IDocumentSession documentSession, string id, object[] events, CancellationToken ct)
        where T : class
    {
        if (events.Length == 0)
            return Task.CompletedTask;

        documentSession.Events.StartStream<T>(id, events);
        return documentSession.SaveChangesAsync(token: ct);
    }

    public static Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        string id,
        int version,
        Func<T, object[]> handle,
        CancellationToken ct
    ) where T : class =>
        documentSession.Events.WriteToAggregate<T>(id, version, stream =>
        {
            var result = handle(stream.Aggregate);
            if (result.Length != 0)
                stream.AppendMany(result);
        }, ct);
}
