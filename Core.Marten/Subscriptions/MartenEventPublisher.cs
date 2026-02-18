using Core.Events;
using Core.OpenTelemetry;
using Marten;
using Marten.Events.Daemon;
using Marten.Events.Daemon.Internals;
using Marten.Storage;
using Marten.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Core.Marten.Subscriptions;

public class MartenEventPublisher(
    IServiceProvider serviceProvider,
    IActivityScope activityScope,
    ILogger<MartenEventPublisher> logger
): SubscriptionBase
{
    public override async Task<IChangeListener> ProcessEventsAsync(
        EventRange eventRange,
        ISubscriptionController subscriptionController,
        IDocumentOperations operations,
        CancellationToken token
    )
    {
        var lastProcessed = eventRange.SequenceFloor;
        try
        {
            foreach (var @event in eventRange.Events)
            {
                var parentContext =
                    TelemetryPropagator.Extract(@event.Headers, ExtractTraceContextFromEventMetadata);

                await activityScope.Run($"{nameof(MartenEventPublisher)}/{nameof(ProcessEventsAsync)}",
                    async (_, ct) =>
                    {
                        using var scope = serviceProvider.CreateScope();
                        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

                        var eventMetadata = new EventMetadata(
                            @event.Id.ToString(),
                            (ulong)@event.Version,
                            (ulong)@event.Sequence,
                            parentContext
                        );

                        await eventBus.Publish(EventEnvelope.From(@event.Data, eventMetadata), ct)
                            .ConfigureAwait(false);

                        //  TODO:  you can also differentiate based on the exception
                        // await controller.RecordDeadLetterEventAsync(e, ex);
                    },
                    new StartActivityOptions
                    {
                        Tags = { { TelemetryTags.EventHandling.Event, @event.Data.GetType() } },
                        Parent = parentContext.ActivityContext
                    },
                    token
                ).ConfigureAwait(false);
            }

            return NullChangeListener.Instance;
        }
        catch (Exception exc)
        {
            logger.LogError("Error while processing Marten Subscription: {ExceptionMessage}", exc.Message);
            await subscriptionController.ReportCriticalFailureAsync(exc, lastProcessed).ConfigureAwait(false);
            throw;
        }
    }

    private IEnumerable<string> ExtractTraceContextFromEventMetadata(Dictionary<string, object>? headers, string key)
    {
        try
        {
            if (headers!.TryGetValue(key, out var value) != true)
                return [];

            var stringValue = value.ToString();

            return stringValue != null
                ? new[] { stringValue }
                : Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to extract trace context: {ex}", ex);
            return [];
        }
    }
}
