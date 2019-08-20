using System;
using System.Linq;
using Marten;

namespace EventStoreBasics
{
    public class MartenRepository<T>: IRepository<T> where T : class, IAggregate, new()
    {
        private readonly IDocumentSession documentSession;

        public MartenRepository(IDocumentSession documentSession)
        {
            this.documentSession = documentSession ?? throw new ArgumentNullException(nameof(documentSession));
        }

        public T Find(Guid id)
        {
            return documentSession.Events.AggregateStream<T>(id);
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
}
