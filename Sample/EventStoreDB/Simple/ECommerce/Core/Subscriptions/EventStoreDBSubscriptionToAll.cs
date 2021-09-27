using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Core.Events;
using ECommerce.Core.Serialisation;
using ECommerce.Core.Threading;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.Core.Subscriptions
{
    public class EventStoreDBSubscriptionToAll
    {
        private readonly IServiceProvider serviceProvider;
        private readonly EventStoreClient eventStoreClient;
        private readonly ISubscriptionCheckpointRepository checkpointRepository;
        private readonly ILogger<EventStoreDBSubscriptionToAll> logger;
        private readonly string subscriptionId;
        private readonly SubscriptionFilterOptions? filterOptions;
        private readonly Action<EventStoreClientOperationOptions>? configureOperation;
        private readonly UserCredentials? credentials;
        private readonly object resubscribeLock = new();
        private CancellationToken? cancellationToken;

        public EventStoreDBSubscriptionToAll(
            IServiceProvider serviceProvider,
            EventStoreClient eventStoreClient,
            ISubscriptionCheckpointRepository checkpointRepository,
            ILogger<EventStoreDBSubscriptionToAll> logger,
            string subscriptionId,
            SubscriptionFilterOptions? filterOptions = null,
            Action<EventStoreClientOperationOptions>? configureOperation = null,
            UserCredentials? credentials = null
        )
        {
            this.serviceProvider = serviceProvider?? throw new ArgumentNullException(nameof(serviceProvider));
            this.eventStoreClient = eventStoreClient ?? throw new ArgumentNullException(nameof(eventStoreClient));
            this.checkpointRepository = checkpointRepository?? throw new ArgumentNullException(nameof(checkpointRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.subscriptionId = subscriptionId;
            this.configureOperation = configureOperation;
            this.credentials = credentials;
            this.filterOptions = filterOptions ?? new SubscriptionFilterOptions(EventTypeFilter.ExcludeSystemEvents());
        }

        private async Task SubscribeToAll(CancellationToken ct)
        {
            cancellationToken = ct;

            logger.LogInformation("Subscription to all '{SubscriptionId}'", subscriptionId);

            var checkpoint = await checkpointRepository.Load(subscriptionId, ct);

            if (checkpoint != null)
            {
                await eventStoreClient.SubscribeToAllAsync(
                    new Position(checkpoint.Value, checkpoint.Value),
                    HandleEvent,
                    false,
                    HandleDrop,
                    filterOptions,
                    configureOperation,
                    credentials,
                    ct
                );
            }
            else
            {
                await eventStoreClient.SubscribeToAllAsync(
                    HandleEvent,
                    false,
                    HandleDrop,
                    filterOptions,
                    configureOperation,
                    credentials,
                    ct
                );
            }

            logger.LogInformation("Subscription to all '{SubscriptionId}' started", subscriptionId);
        }

        private async Task HandleEvent(StreamSubscription subscription, ResolvedEvent resolvedEvent, CancellationToken ct)
        {
            try
            {
                if (IsEventWithEmptyData(resolvedEvent) || IsCheckpointEvent(resolvedEvent)) return;

                // Create scope to have proper handling of scoped services
                using var scope = serviceProvider.CreateScope();

                var eventBus =
                    scope.ServiceProvider.GetRequiredService<IEventBus>();

                // publish event to internal event bus
                await eventBus.Publish(resolvedEvent.Deserialize(), ct);

                await checkpointRepository.Store(subscriptionId, resolvedEvent.Event.Position.CommitPosition, ct);
            }
            catch (Exception e)
            {
                logger.LogError("Error consuming message: {ExceptionMessage}{ExceptionStackTrace}", e.Message, e.StackTrace);
            }
        }

        private bool IsEventWithEmptyData(ResolvedEvent resolvedEvent)
        {
            if (resolvedEvent.Event.Data.Length != 0) return false;

            logger.LogInformation("Event without data received");
            return true;

        }

        private bool IsCheckpointEvent(ResolvedEvent resolvedEvent)
        {
            if (resolvedEvent.Event.EventType != EventTypeMapper.ToName<CheckpointStored>()) return false;

            logger.LogInformation("Checkpoint event - ignoring");
            return true;

        }

        private void HandleDrop(StreamSubscription _, SubscriptionDroppedReason reason, Exception? exception)
        {
            logger.LogWarning(
                exception,
                "Subscription to all '{SubscriptionId}' dropped with '{Reason}'",
                subscriptionId,
                reason
            );

            Resubscribe();
        }

        private void Resubscribe()
        {
            while (true)
            {
                var resubscribed = false;
                try
                {
                    Monitor.Enter(resubscribeLock);

                    using (NoSynchronizationContextScope.Enter())
                    {
                        SubscribeToAll(cancellationToken!.Value).Wait();
                    }

                    resubscribed = true;
                }
                catch (Exception exception)
                {
                    logger.LogWarning(exception, "Failed to resubscribe to all '{SubscriptionId}' dropped with '{ExceptionMessage}{ExceptionStackTrace}'", subscriptionId, exception.Message, exception.StackTrace);
                }
                finally
                {
                    Monitor.Exit(resubscribeLock);
                }

                if (resubscribed)
                    break;

                Thread.Sleep(1000);
            }
        }
    }
}
