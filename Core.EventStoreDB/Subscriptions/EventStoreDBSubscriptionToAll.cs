using System.Diagnostics;
using Core.Events;
using Core.EventStoreDB.Events;
using Core.OpenTelemetry;
using Core.Threading;
using EventStore.Client;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using EventTypeFilter = EventStore.Client.EventTypeFilter;

namespace Core.EventStoreDB.Subscriptions;

public class EventStoreDBSubscriptionToAllOptions
{
    public string SubscriptionId { get; set; } = "default";

    public SubscriptionFilterOptions FilterOptions { get; set; } =
        new(EventTypeFilter.ExcludeSystemEvents());

    public Action<EventStoreClientOperationOptions>? ConfigureOperation { get; set; }
    public UserCredentials? Credentials { get; set; }
    public bool ResolveLinkTos { get; set; }
    public bool IgnoreDeserializationErrors { get; set; } = true;
}

public class EventStoreDBSubscriptionToAll
{
    private readonly IEventBus eventBus;
    private readonly EventStoreClient eventStoreClient;
    private readonly EventTypeMapper eventTypeMapper;
    private readonly ISubscriptionCheckpointRepository checkpointRepository;
    private readonly IActivityScope activityScope;
    private readonly ILogger<EventStoreDBSubscriptionToAll> logger;
    private EventStoreDBSubscriptionToAllOptions subscriptionOptions = default!;
    private string SubscriptionId => subscriptionOptions.SubscriptionId;
    private readonly object resubscribeLock = new();
    private CancellationToken cancellationToken;

    public EventStoreDBSubscriptionToAll(
        EventStoreClient eventStoreClient,
        EventTypeMapper eventTypeMapper,
        IEventBus eventBus,
        ISubscriptionCheckpointRepository checkpointRepository,
        IActivityScope activityScope,
        ILogger<EventStoreDBSubscriptionToAll> logger
    )
    {
        this.eventBus = eventBus;
        this.eventStoreClient = eventStoreClient;
        this.eventTypeMapper = eventTypeMapper;
        this.checkpointRepository = checkpointRepository;
        this.activityScope = activityScope;
        this.logger = logger;
    }

    public async Task SubscribeToAll(EventStoreDBSubscriptionToAllOptions subscriptionOptions, CancellationToken ct)
    {
        // see: https://github.com/dotnet/runtime/issues/36063
        await Task.Yield();

        this.subscriptionOptions = subscriptionOptions;
        cancellationToken = ct;

        logger.LogInformation("Subscription to all '{SubscriptionId}'", subscriptionOptions.SubscriptionId);

        var checkpoint = await checkpointRepository.Load(SubscriptionId, ct).ConfigureAwait(false);

        await eventStoreClient.SubscribeToAllAsync(
            checkpoint == null ? FromAll.Start : FromAll.After(new Position(checkpoint.Value, checkpoint.Value)),
            HandleEvent,
            subscriptionOptions.ResolveLinkTos,
            HandleDrop,
            subscriptionOptions.FilterOptions,
            subscriptionOptions.Credentials,
            ct
        ).ConfigureAwait(false);

        logger.LogInformation("Subscription to all '{SubscriptionId}' started", SubscriptionId);
    }

    private async Task HandleEvent(
        StreamSubscription subscription,
        ResolvedEvent resolvedEvent,
        CancellationToken token
    )
    {
        try
        {
            if (IsEventWithEmptyData(resolvedEvent) || IsCheckpointEvent(resolvedEvent)) return;

            var eventEnvelope = resolvedEvent.ToEventEnvelope();

            if (eventEnvelope == null)
            {
                // That can happen if we're sharing database between modules.
                // If we're subscribing to all and not filtering out events from other modules,
                // then we might get events that are from other module and we might not be able to deserialize them.
                // In that case it's safe to ignore deserialization error.
                // You may add more sophisticated logic checking if it should be ignored or not.
                logger.LogWarning("Couldn't deserialize event with id: {EventId}", resolvedEvent.Event.EventId);

                if (!subscriptionOptions.IgnoreDeserializationErrors)
                    throw new InvalidOperationException(
                        $"Unable to deserialize event {resolvedEvent.Event.EventType} with id: {resolvedEvent.Event.EventId}"
                    );

                return;
            }

            await activityScope.Run($"{nameof(EventStoreDBSubscriptionToAll)}/{nameof(HandleEvent)}",
                async (_, ct) =>
                {
                    // publish event to internal event bus
                    await eventBus.Publish(eventEnvelope, ct).ConfigureAwait(false);

                    await checkpointRepository.Store(SubscriptionId, resolvedEvent.Event.Position.CommitPosition, ct)
                        .ConfigureAwait(false);
                },
                new StartActivityOptions
                {
                    Tags = { { TelemetryTags.EventHandling.Event, eventEnvelope.Data.GetType() } },
                    Parent = eventEnvelope.Metadata.PropagationContext?.ActivityContext,
                    Kind = ActivityKind.Consumer
                },
                token
            ).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError("Error consuming message: {ExceptionMessage}{ExceptionStackTrace}", e.Message,
                e.StackTrace);
            // if you're fine with dropping some events instead of stopping subscription
            // then you can add some logic if error should be ignored
            throw;
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

        if (exception is RpcException { StatusCode: StatusCode.Cancelled })
            return;

        Resubscribe();
    }

    private void Resubscribe()
    {
        // You may consider adding a max resubscribe count if you want to fail process
        // instead of retrying until database is up
        while (true)
        {
            var resubscribed = false;
            try
            {
                Monitor.Enter(resubscribeLock);

                // No synchronization context is needed to disable synchronization context.
                // That enables running asynchronous method not causing deadlocks.
                // As this is a background process then we don't need to have async context here.
                using (NoSynchronizationContextScope.Enter())
                {
#pragma warning disable VSTHRD002
                    SubscribeToAll(subscriptionOptions, cancellationToken).Wait(cancellationToken);
#pragma warning restore VSTHRD002
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

            // Sleep between reconnections to not flood the database or not kill the CPU with infinite loop
            // Randomness added to reduce the chance of multiple subscriptions trying to reconnect at the same time
            Thread.Sleep(1000 + new Random((int)DateTime.UtcNow.Ticks).Next(1000));
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
        if (resolvedEvent.Event.EventType != eventTypeMapper.ToName<CheckpointStored>()) return false;

        logger.LogInformation("Checkpoint event - ignoring");
        return true;
    }
}
