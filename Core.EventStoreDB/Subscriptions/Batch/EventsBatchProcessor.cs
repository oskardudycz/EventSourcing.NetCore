using System.Diagnostics;
using Core.Events;
using Core.EventStoreDB.Events;
using Core.EventStoreDB.Subscriptions.Checkpoints;
using Core.OpenTelemetry;
using EventStore.Client;
using Microsoft.Extensions.Logging;

namespace Core.EventStoreDB.Subscriptions.Batch;

public class EventsBatchProcessor(
    EventTypeMapper eventTypeMapper,
    IEventBus eventBus,
    IActivityScope activityScope,
    ILogger<EventsBatchProcessor> logger
)
{
    public async Task<ulong?> HandleEventsBatch(
        ResolvedEvent[] resolvedEvents,
        EventStoreDBSubscriptionToAllOptions options,
        CancellationToken ct
    )
    {
        var events = TryDeserializeEvents(resolvedEvents, options.IgnoreDeserializationErrors);
        ulong? lastPosition = null;

        foreach (var @event in events)
        {
            await HandleEvent(@event, ct).ConfigureAwait(false);
            lastPosition = @event.Metadata.LogPosition;
        }

        return lastPosition;
    }

    private IEventEnvelope[] TryDeserializeEvents(
        ResolvedEvent[] resolvedEvents,
        bool ignoreDeserializationErrors
    )
    {
        List<IEventEnvelope> result = [];

        foreach (var resolvedEvent in resolvedEvents)
        {
            if (IsEventWithEmptyData(resolvedEvent) || IsCheckpointEvent(resolvedEvent)) continue;

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

                if (!ignoreDeserializationErrors)
                    throw new InvalidOperationException(
                        $"Unable to deserialize event {resolvedEvent.Event.EventType} with id: {resolvedEvent.Event.EventId}"
                    );
                continue;
            }

            result.Add(eventEnvelope);
        }

        return result.ToArray();
    }

    private async Task HandleEvent(
        IEventEnvelope eventEnvelope,
        CancellationToken token
    )
    {
        try
        {
            await activityScope.Run($"{nameof(EventStoreDBSubscriptionToAll)}/{nameof(HandleEvent)}",
                async (_, ct) =>
                {
                    // publish event to internal event bus
                    await eventBus.Publish(eventEnvelope, ct).ConfigureAwait(false);
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
