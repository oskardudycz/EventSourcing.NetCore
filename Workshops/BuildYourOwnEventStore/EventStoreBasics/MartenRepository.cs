using Marten;

namespace EventStoreBasics;

public class MartenRepository<T>(IDocumentSession documentSession): IRepository<T>
    where T : class, IAggregate, new()
{
    private readonly IDocumentSession documentSession = documentSession ?? throw new ArgumentNullException(nameof(documentSession));

    public T Find(Guid id)
    {
        var aggregate = documentSession.Events.AggregateStream<T>(id);

        if (aggregate == default)
            throw new ArgumentNullException(nameof(aggregate));

        return aggregate;
    }

    public void Add(T aggregate)
    {
        documentSession.Events.StartStream<T>(
            aggregate.Id,
            aggregate.DequeueUncommittedEvents().ToArray()
        );
        documentSession.SaveChanges();
    }

    public void Update(T aggregate)
    {
        documentSession.Events.Append(
            aggregate.Id,
            aggregate.Version,
            aggregate.DequeueUncommittedEvents().ToArray()
        );
        documentSession.SaveChanges();
    }

    public void Delete(T aggregate)
    {
        documentSession.Events.Append(
            aggregate.Id,
            aggregate.Version,
            aggregate.DequeueUncommittedEvents().ToArray()
        );
        documentSession.SaveChanges();
    }
}
