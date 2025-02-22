using Marten;

namespace EventStoreBasics;

public class MartenRepository<T>(IDocumentSession documentSession): IRepository<T>
    where T : class, IAggregate, new()
{
    private readonly IDocumentSession documentSession = documentSession ?? throw new ArgumentNullException(nameof(documentSession));

    public Task<T?> Find(Guid id, CancellationToken ct = default) =>
        documentSession.Events.AggregateStreamAsync<T>(id, token: ct);

    public Task Add(T aggregate, CancellationToken ct = default)
    {
        documentSession.Events.StartStream<T>(
            aggregate.Id,
            aggregate.DequeueUncommittedEvents().ToArray()
        );
        return documentSession.SaveChangesAsync(ct);
    }

    public Task Update(T aggregate, CancellationToken ct = default)
    {
        documentSession.Events.Append(
            aggregate.Id,
            aggregate.Version,
            aggregate.DequeueUncommittedEvents().ToArray()
        );
        return documentSession.SaveChangesAsync(ct);
    }

    public Task Delete(T aggregate, CancellationToken ct = default)
    {
        documentSession.Events.Append(
            aggregate.Id,
            aggregate.Version,
            aggregate.DequeueUncommittedEvents().ToArray()
        );
        return documentSession.SaveChangesAsync(ct);
    }
}
