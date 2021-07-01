using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Events;
using Core.Events.External;
using Core.EventStoreDB.Serialization;
using Core.Reflection;
using Core.Subscriptions;
using Core.Threading;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Core.EventStoreDB.Subscriptions
{
    //See more: https://www.stevejgordon.co.uk/asp-net-core-2-ihostedservice
    public class SubscribeToAllBackgroundWorker: IHostedService
    {
        private Task? executingTask;
        private CancellationTokenSource? cts;
        private readonly IServiceProvider serviceProvider;
        private readonly EventStoreClient eventStoreClient;
        private readonly ISubscriptionCheckpointRepository checkpointRepository;
        private readonly ILogger<SubscribeToAllBackgroundWorker> logger;
        private readonly string subscriptionId;
        private readonly SubscriptionFilterOptions? filterOptions;
        private readonly Action<EventStoreClientOperationOptions>? configureOperation;
        private readonly UserCredentials? credentials;

        public SubscribeToAllBackgroundWorker(
            IServiceProvider serviceProvider,
            EventStoreClient eventStoreClient,
            ISubscriptionCheckpointRepository checkpointRepository,
            ILogger<SubscribeToAllBackgroundWorker> logger,
            string subscriptionId,
            SubscriptionFilterOptions? filterOptions = null,
            Action<EventStoreClientOperationOptions>? configureOperation = null,
            UserCredentials? credentials = null
        )
        {
            this.serviceProvider = serviceProvider?? throw new ArgumentNullException(nameof(eventStoreClient));
            this.eventStoreClient = eventStoreClient ?? throw new ArgumentNullException(nameof(eventStoreClient));
            this.checkpointRepository = checkpointRepository?? throw new ArgumentNullException(nameof(eventStoreClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.subscriptionId = subscriptionId;
            this.configureOperation = configureOperation;
            this.credentials = credentials;
            this.filterOptions = filterOptions ?? new SubscriptionFilterOptions(EventTypeFilter.ExcludeSystemEvents());
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("External Event Consumer started");

            // Create a linked token so we can trigger cancellation outside of this token's cancellation
            cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var checkpoint = await checkpointRepository.Load(subscriptionId, cts.Token);

            executingTask = SubscribeToAll(checkpoint, cts.Token);

            await executingTask;

            logger.LogInformation("Subscription to all '{SubscriptionId}' started", subscriptionId);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Stop called without start
            if (executingTask == null)
            {
                return;
            }

            // Signal cancellation to the executing method
            cts?.Cancel();

            // Wait until the issue completes or the stop token triggers
            await Task.WhenAny(executingTask, Task.Delay(-1, cancellationToken));

            // Throw if cancellation triggered
            cancellationToken.ThrowIfCancellationRequested();

            logger.LogInformation("Subscription to all '{SubscriptionId}' stoped", subscriptionId);
            logger.LogInformation("External Event Consumer stopped");
        }

        private Task SubscribeToAll(ulong? checkpoint, CancellationToken ct)
        {
            return checkpoint != null
                ? eventStoreClient.SubscribeToAllAsync(
                    new Position(checkpoint.Value, checkpoint.Value),
                    HandleEvent,
                    false,
                    HandleDrop,
                    filterOptions,
                    configureOperation,
                    credentials,
                    ct
                )
                : eventStoreClient.SubscribeToAllAsync(
                    HandleEvent,
                    false,
                    HandleDrop,
                    filterOptions,
                    configureOperation,
                    credentials,
                    ct
                );
        }

        private async Task HandleEvent(StreamSubscription subscription, ResolvedEvent resolvedEvent, CancellationToken ct)
        {
            try
            {
                if (IsEventWithEmptyData(resolvedEvent) || IsCheckpointEvent(resolvedEvent)) return;

                using var scope = serviceProvider.CreateScope();

                var eventBus =
                    scope.ServiceProvider.GetRequiredService<IEventBus>();

                // publish event to internal event bus
                await eventBus.Publish((IEvent)resolvedEvent.Deserialize());

                await checkpointRepository.Store(subscriptionId, resolvedEvent.Event.Position.CommitPosition, ct);
            }
            catch (Exception e)
            {
                logger.LogInformation("Error consuming message: {ExceptionMessage}{ExceptionStackTrace}", e.Message, e.StackTrace);
            }
        }

        private bool IsEventWithEmptyData(ResolvedEvent resolvedEvent)
        {
            if (resolvedEvent.Event.Data.Length == 0)
            {
                logger.LogInformation("Event without data received");
                return true;
            }

            return false;
        }

        private bool IsCheckpointEvent(ResolvedEvent resolvedEvent)
        {
            if (resolvedEvent.Event.EventType == EventTypeMapper.ToName<CheckpointStored>())
            {
                logger.LogInformation("Checkpoint event - ignoring");
                return true;
            }

            return false;
        }

        private void HandleDrop(StreamSubscription subscription, SubscriptionDroppedReason reason, Exception? exception)
        {
            logger.LogWarning(
                exception,
                "Subscription to all '{SubscriptionId}' dropped with '{Reason}'",
                subscriptionId,
                reason
            );

            using (NoSynchronizationContextScope.Enter())
            {
                Resubscribe(subscription).Wait();
            }
        }

        private Task Resubscribe(StreamSubscription subscription)
        {
            // TODO: Add resubscribe code here
            return Task.CompletedTask;
        }

    }
}
