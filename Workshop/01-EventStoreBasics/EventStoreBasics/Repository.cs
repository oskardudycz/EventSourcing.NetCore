using System;
using System.Linq;

namespace EventStoreBasics
{
    public class Repository<T>: IRepository<T> where T: IAggregate
    {
        private readonly IEventStore eventStore;

        public Repository(IEventStore eventStore)
        {
            this.eventStore = eventStore ?? throw  new ArgumentNullException(nameof(eventStore));
        }

        public T Find(Guid id)
        {
            return eventStore.AggregateStream<T>(id);
        }

        public void Add(T aggregate)
        {
            eventStore.Store(aggregate);
        }

        public void Update(T aggregate)
        {
            eventStore.Store(aggregate);
        }

        public void Delete(T aggregate)
        {
            eventStore.Store(aggregate);
        }
    }
}
