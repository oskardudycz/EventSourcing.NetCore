using System.Diagnostics;
using Core.OpenTelemetry;
using Microsoft.Extensions.Logging;

namespace Core.Events;

public class EventBusBatchHandler(
    IEventBus eventBus,
    IActivityScope activityScope,
    ILogger<EventBusBatchHandler> logger
): IEventBatchHandler
{
    public async Task Handle(IEventEnvelope[] eventInEnvelopes, CancellationToken ct)
    {
        foreach (var @event in eventInEnvelopes)
        {
            await HandleEvent(@event, ct).ConfigureAwait(false);
        }
    }

    private async Task HandleEvent(
        IEventEnvelope eventEnvelope,
        CancellationToken token
    )
    {
        try
        {
            await activityScope.Run($"{nameof(EventBusBatchHandler)}/{nameof(HandleEvent)}",
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
}
