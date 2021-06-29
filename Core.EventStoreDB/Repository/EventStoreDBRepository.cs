using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Aggregates;
using Core.Events;
using Core.Reflection;
using Core.Repositories;
using EventStore.Client;
using Newtonsoft.Json;

namespace Core.EventStoreDB.Repository
{
    public class EventStoreDBRepository<T>: IRepository<T> where T : class, IAggregate
    {
        private readonly EventStoreClient eventStore;
        private readonly IEventBus eventBus;

        public EventStoreDBRepository(
            EventStoreClient eventStoreDBClient,
            IEventBus eventBus
        )
        {
            this.eventStore = eventStoreDBClient ?? throw new ArgumentNullException(nameof(eventStoreDBClient));
            this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public async Task<T?> Find(Guid id, CancellationToken cancellationToken)
        {
            var readResult = eventStore.ReadStreamAsync(
                Direction.Forwards,
                // TODO: Inject stream generation strategy
                $"{typeof(T).Name}-{id}",
                StreamPosition.Start,
                cancellationToken: cancellationToken
            );

            // TODO: consider adding extension method for the aggregation and deserialisation
            var aggregate = (T)Activator.CreateInstance(typeof(T), true)!;

            await foreach (var @event in readResult)
            {
                // get event type
                var eventType = TypeProvider.GetTypeFromAnyReferencingAssembly(@event.Event.EventType);

                // deserialize event
                var eventData = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(@event.Event.Data.Span), eventType!);

                aggregate.When(eventData!);
            }

            return aggregate;
        }

        public Task Add(T aggregate, CancellationToken cancellationToken)
        {
            return Store(aggregate, cancellationToken);
        }

        public Task Update(T aggregate, CancellationToken cancellationToken)
        {
            return Store(aggregate, cancellationToken);
        }

        public Task Delete(T aggregate, CancellationToken cancellationToken)
        {
            return Store(aggregate, cancellationToken);
        }

        private async Task Store(T aggregate, CancellationToken cancellationToken)
        {
            var events = aggregate.DequeueUncommittedEvents();

            var eventsToStore = aggregate.DequeueUncommittedEvents()
                .Select(
                    @event => new EventData(
                        Uuid.NewUuid(),
                        @event.GetType().FullName!,
                        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
                        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new {}))
                    )
                ).ToArray();

            await eventStore.AppendToStreamAsync(
                $"{typeof(T).Name}-{aggregate.Id}",
                // TODO: Add proper optimistic concurrency handling
                StreamState.Any,
                eventsToStore,
                cancellationToken: cancellationToken
            );

            await eventBus.Publish(events);
        }
    }
}
