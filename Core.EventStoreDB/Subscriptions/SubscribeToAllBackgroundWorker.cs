using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Events;
using Core.EventStoreDB.Serialization;
using Core.Threading;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Core.EventStoreDB.Subscriptions;

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
    private readonly object resubscribeLock = new();

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

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Create a linked token so we can trigger cancellation outside of this token's cancellation
        cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        executingTask = SubscribeToAll(cts.Token);

        return executingTask;
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

        logger.LogInformation("Subscription to all '{SubscriptionId}' stopped", subscriptionId);
        logger.LogInformation("External Event Consumer stopped");
    }

    private async Task SubscribeToAll(CancellationToken ct)
    {
        logger.LogInformation("Subscription to all '{SubscriptionId}'", subscriptionId);

        var checkpoint = await checkpointRepository.Load(subscriptionId, ct);

        await eventStoreClient.SubscribeToAllAsync(
            checkpoint == null? FromAll.Start : FromAll.After(new Position(checkpoint.Value, checkpoint.Value)),
            HandleEvent,
            false,
            HandleDrop,
            filterOptions,
            credentials,
            ct
        );

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
            await eventBus.Publish((IEvent)resolvedEvent.Deserialize());

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
                    SubscribeToAll(cts!.Token).Wait();
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
