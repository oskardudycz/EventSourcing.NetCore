namespace EventStoreBasics;

public class Repository<T>(IEventStore eventStore): IRepository<T>
    where T : IAggregate
{
    private readonly IEventStore eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));

    public T Find(Guid id)
    {
        throw new NotImplementedException("Get aggregate state by aggregating stream");
    }

    public void Add(T aggregate)
    {
        throw new NotImplementedException("Add new stream and events");
    }

    public void Update(T aggregate)
    {
        throw new NotImplementedException("Add new events to existing stream");
    }

    public void Delete(T aggregate)
    {
        throw new NotImplementedException("Add delete event to stream");
    }
}
