using Marten;

namespace Helpdesk.Api.Core.Marten;

public static class DocumentSessionExtensions
{
    public static Task Add<T>(this IDocumentSession documentSession, Guid id, object @event, CancellationToken ct)
        where T : class
    {
        documentSession.Events.StartStream<T>(id, @event);
        return documentSession.SaveChangesAsync(token: ct);
    }

    public static Task GetAndUpdate<T>(this IDocumentSession documentSession, Guid id, int version,
        Func<T, object> handle, CancellationToken ct)
        where T : class =>
        documentSession.Events.WriteToAggregate<T>(id, version, stream =>
            stream.AppendOne(handle(stream.Aggregate)), ct);
}
