using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Core.Serialisation;
using ECommerce.Core.Subscriptions;
using EventStore.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Core
{
    public class EventStoreDBConfig
    {
        public string ConnectionString { get; set; } = default!;
    }

    public record EventStoreDBOptions(
        bool UseInternalCheckpointing = true
    );

    public static class EventStoreDBConfigExtensions
    {
        public static async Task<TEntity> Find<TEntity>(
            this EventStoreClient eventStore,
            Func<TEntity> getDefault,
            Func<TEntity, object, TEntity> when,
            string id,
            CancellationToken cancellationToken)
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
                    getDefault(),
                    when,
                    cancellationToken
                ))!;
        }

        public static async Task Append(
            this EventStoreClient eventStore,
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


        public static async Task Append(
            this EventStoreClient eventStore,
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


        private const string DefaultConfigKey = "EventStore";

        public static IServiceCollection AddEventStoreDB(this IServiceCollection services, IConfiguration config, EventStoreDBOptions? options = null)
        {
            var eventStoreDBConfig = config.GetSection(DefaultConfigKey).Get<EventStoreDBConfig>();

            services.AddSingleton(
                    new EventStoreClient(EventStoreClientSettings.Create(eventStoreDBConfig.ConnectionString)))
                .AddTransient<EventStoreDBSubscriptionToAll, EventStoreDBSubscriptionToAll>();

            if (options?.UseInternalCheckpointing != false)
            {
                services
                    .AddTransient<ISubscriptionCheckpointRepository, EventStoreDBSubscriptionCheckpointRepository>();
            }

            return services;
        }
    }
}
