using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Core.Events;
using ECommerce.Core.Serialisation;
using ECommerce.Core.Threading;
using EventStore.Client;
using Microsoft.Extensions.Logging;

namespace ECommerce.Core.Subscriptions
{
    public class EventStoreDBSubscriptionToAllOptions
    {
        public string SubscriptionId { get; set; } = "default";

        public SubscriptionFilterOptions FilterOptions { get; set; } =
            new(EventTypeFilter.ExcludeSystemEvents());

        public Action<EventStoreClientOperationOptions>? ConfigureOperation { get; set; }
        public UserCredentials? Credentials { get; set; }

        public bool ResolveLinkTos { get; set; }
    }

    public class EventStoreDBSubscriptionToAll
    {
        private readonly IEventBus eventBus;
        private readonly EventStoreClient eventStoreClient;
        private readonly ISubscriptionCheckpointRepository checkpointRepository;
        private readonly ILogger<EventStoreDBSubscriptionToAll> logger;
        private EventStoreDBSubscriptionToAllOptions subscriptionOptions = default!;
        private string SubscriptionId => subscriptionOptions.SubscriptionId;
        private readonly object resubscribeLock = new();
        private CancellationToken cancellationToken;

        public EventStoreDBSubscriptionToAll(
            EventStoreClient eventStoreClient,
            IEventBus eventBus,
            ISubscriptionCheckpointRepository checkpointRepository,
            ILogger<EventStoreDBSubscriptionToAll> logger
        )
        {
            this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            this.eventStoreClient = eventStoreClient ?? throw new ArgumentNullException(nameof(eventStoreClient));
            this.checkpointRepository =
                checkpointRepository ?? throw new ArgumentNullException(nameof(checkpointRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SubscribeToAll(EventStoreDBSubscriptionToAllOptions subscriptionOptions, CancellationToken ct)
        {
            this.subscriptionOptions = subscriptionOptions;
            cancellationToken = ct;

            logger.LogInformation("Subscription to all '{SubscriptionId}'", subscriptionOptions.SubscriptionId);

            var checkpoint = await checkpointRepository.Load(SubscriptionId, ct);

            if (checkpoint != null)
            {
                await eventStoreClient.SubscribeToAllAsync(
                    new Position(checkpoint.Value, checkpoint.Value),
                    HandleEvent,
                    subscriptionOptions.ResolveLinkTos,
                    HandleDrop,
                    subscriptionOptions.FilterOptions,
                    subscriptionOptions.ConfigureOperation,
                    subscriptionOptions.Credentials,
                    ct
                );
            }
            else
            {
                await eventStoreClient.SubscribeToAllAsync(
                    HandleEvent,
                    false,
                    HandleDrop,
                    subscriptionOptions.FilterOptions,
                    subscriptionOptions.ConfigureOperation,
                    subscriptionOptions.Credentials,
                    ct
                );
            }

            logger.LogInformation("Subscription to all '{SubscriptionId}' started", SubscriptionId);
        }

        private async Task HandleEvent(StreamSubscription subscription, ResolvedEvent resolvedEvent,
            CancellationToken ct)
        {
            try
            {
                if (IsEventWithEmptyData(resolvedEvent) || IsCheckpointEvent(resolvedEvent)) return;

                // publish event to internal event bus
                await eventBus.Publish(resolvedEvent.Deserialize(), ct);

                await checkpointRepository.Store(SubscriptionId, resolvedEvent.Event.Position.CommitPosition, ct);
            }
            catch (Exception e)
            {
                logger.LogError("Error consuming message: {ExceptionMessage}{ExceptionStackTrace}", e.Message,
                    e.StackTrace);
            }
        }

        private void HandleDrop(StreamSubscription _, SubscriptionDroppedReason reason, Exception? exception)
        {
            logger.LogError(
                exception,
                "Subscription to all '{SubscriptionId}' dropped with '{Reason}'",
                SubscriptionId,
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
                        SubscribeToAll(subscriptionOptions, cancellationToken).Wait(cancellationToken);
                    }

                    resubscribed = true;
                }
                catch (Exception exception)
                {
                    logger.LogWarning(exception,
                        "Failed to resubscribe to all '{SubscriptionId}' dropped with '{ExceptionMessage}{ExceptionStackTrace}'",
                        SubscriptionId, exception.Message, exception.StackTrace);
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
    }
}
