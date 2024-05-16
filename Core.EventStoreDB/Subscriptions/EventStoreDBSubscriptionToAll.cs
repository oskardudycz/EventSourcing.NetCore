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

public class EventStoreDBSubscriptionToAll(
    EventStoreClient eventStoreClient,
    EventTypeMapper eventTypeMapper,
    IEventBus eventBus,
    ISubscriptionCheckpointRepository checkpointRepository,
    IActivityScope activityScope,
    ILogger<EventStoreDBSubscriptionToAll> logger
)
{
    private EventStoreDBSubscriptionToAllOptions subscriptionOptions = default!;
    private string SubscriptionId => subscriptionOptions.SubscriptionId;
    private readonly object resubscribeLock = new();
    private CancellationToken cancellationToken;

    public async Task SubscribeToAll(EventStoreDBSubscriptionToAllOptions subscriptionOptions, CancellationToken ct)
    {
        // see: https://github.com/dotnet/runtime/issues/36063
        await Task.Yield();

        this.subscriptionOptions = subscriptionOptions;
        cancellationToken = ct;

        logger.LogInformation("Subscription to all '{SubscriptionId}'", subscriptionOptions.SubscriptionId);

        var checkpoint = await checkpointRepository.Load(SubscriptionId, ct).ConfigureAwait(false);

        var subscription = eventStoreClient.SubscribeToAll(
            checkpoint == null
                ? FromAll.Start
                : FromAll.After(new Position(checkpoint.Value, checkpoint.Value)),
            subscriptionOptions.ResolveLinkTos,
            subscriptionOptions.FilterOptions,
            subscriptionOptions.Credentials,
            ct
        );

        logger.LogInformation("Subscription to all '{SubscriptionId}' started", SubscriptionId);

        try
        {
            await foreach (var @event in subscription)
            {
                await HandleEvent(@event, ct).ConfigureAwait(false);
            }
        }
        catch (RpcException rpcException) when (rpcException is { StatusCode: StatusCode.Cancelled } ||
                                                rpcException.InnerException is ObjectDisposedException)
        {
            logger.LogWarning(
                "Subscription to all '{SubscriptionId}' dropped by client",
                SubscriptionId
            );
        }
        catch (Exception ex)
        {
            logger.LogWarning("Subscription was dropped: {ex}", ex);

            // Sleep between reconnections to not flood the database or not kill the CPU with infinite loop
            // Randomness added to reduce the chance of multiple subscriptions trying to reconnect at the same time
            Thread.Sleep(1000 + new Random((int)DateTime.UtcNow.Ticks).Next(1000));

            await SubscribeToAll(this.subscriptionOptions, ct).ConfigureAwait(false);
        }
    }

    private async Task HandleEvent(
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
                logger.LogWarning("Couldn't deserialize event with {EventType} and id: {EventId}",
                    resolvedEvent.Event.EventType, resolvedEvent.Event.EventId);

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
