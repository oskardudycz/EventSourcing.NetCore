using Core.Events;
using Core.EventStoreDB.Events;
using Core.EventStoreDB.Subscriptions.Checkpoints;
using EventStore.Client;
using Microsoft.Extensions.Logging;

namespace Core.EventStoreDB.Subscriptions.Batch;

public record BatchProcessingOptions(
    string SubscriptionId,
    bool IgnoreDeserializationErrors,
    IEventBatchHandler[] BatchHandlers
);

public class EventsBatchProcessor(
    EventTypeMapper eventTypeMapper,
    ILogger<EventsBatchProcessor> logger
)
{
    public async Task Handle(
        ResolvedEvent[] resolvedEvents,
        BatchProcessingOptions options,
        CancellationToken ct
    )
    {
        var events = TryDeserializeEvents(resolvedEvents, options.IgnoreDeserializationErrors);

        foreach (var batchHandler in options.BatchHandlers)
        {
            // TODO: How would you implement Dead-Letter Queue here?
            await batchHandler.Handle(events, ct).ConfigureAwait(false);
        }
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
