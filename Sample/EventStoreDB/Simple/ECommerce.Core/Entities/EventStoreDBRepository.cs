using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Core.Serialisation;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Core.Entities
{
    public interface IEventStoreDBRepository<TEntity> where TEntity: notnull
    {
        Task<TEntity> Find(Func<TEntity?, object, TEntity> when, string id, CancellationToken cancellationToken);

        Task Append(string id, object @event, CancellationToken cancellationToken);

        Task Append(string id, object @event, uint version, CancellationToken cancellationToken);
    }

    public class EventStoreDBRepository<TEntity>: IEventStoreDBRepository<TEntity> where TEntity: class
    {
        private readonly EventStoreClient eventStore;

        public EventStoreDBRepository(
            EventStoreClient eventStore
        )
        {
            this.eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        }

        public async Task<TEntity> Find(
            Func<TEntity?, object, TEntity> when,
            string id,
            CancellationToken cancellationToken
        )
        {
            var readResult = eventStore.ReadStreamAsync(
                Direction.Forwards,
                id,
                StreamPosition.Start,
                cancellationToken: cancellationToken
            );

            return (await readResult
                .Select(@event => @event.Deserialize())
                .AggregateAsync(
                    default,
                    when,
                    cancellationToken
                ))!;
        }

        public async Task Append(
            string id,
            object @event,
            CancellationToken cancellationToken
        )

        {
            await eventStore.AppendToStreamAsync(
                id,
                StreamState.NoStream,
                new[] { @event.ToJsonEventData() },
                cancellationToken: cancellationToken
            );
        }


        public async Task Append(
            string id,
            object @event,
            uint version,
            CancellationToken cancellationToken
        )
        {
            await eventStore.AppendToStreamAsync(
                id,
                StreamRevision.FromInt64(version),
                new[] { @event.ToJsonEventData() },
                cancellationToken: cancellationToken
            );
        }
    }

    public static class EventStoreDBRepository
    {
        public static IServiceCollection AddEventStoreDBRepository<TEntity>(
            this IServiceCollection services
        ) where TEntity : class =>
            services.AddTransient<IEventStoreDBRepository<TEntity>, EventStoreDBRepository<TEntity>>();
    }
}
