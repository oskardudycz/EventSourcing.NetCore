using Core.Events;
using Core.OpenTelemetry;
using Marten;
using Marten.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Core.Marten.Subscriptions;

public class MartenEventPublisher: IMartenEventsConsumer
{
    private readonly IServiceProvider serviceProvider;
    private readonly IActivityScope activityScope;
    private readonly ILogger<MartenEventPublisher> logger;

    public MartenEventPublisher(
        IServiceProvider serviceProvider,
        IActivityScope activityScope,
        ILogger<MartenEventPublisher> logger
    )
    {
        this.serviceProvider = serviceProvider;
        this.activityScope = activityScope;
        this.logger = logger;
    }

    public async Task ConsumeAsync(
        IDocumentOperations documentOperations,
        IReadOnlyList<StreamAction> streamActions,
        CancellationToken cancellationToken
    )
    {
        foreach (var @event in streamActions.SelectMany(streamAction => streamAction.Events))
        {
            var parentContext =
                TelemetryPropagator.Extract(@event.Headers, ExtractTraceContextFromEventMetadata);

            await activityScope.Run($"{nameof(MartenEventPublisher)}/{nameof(ConsumeAsync)}",
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

                    await eventBus.Publish(EventEnvelopeFactory.From(@event.Data, eventMetadata), ct);
                },
                new StartActivityOptions
                {
                    Tags = { { TelemetryTags.EventHandling.Event, @event.Data.GetType() } },
                    Parent = parentContext.ActivityContext
                },
                cancellationToken
            );
        }
    }

    private IEnumerable<string> ExtractTraceContextFromEventMetadata(Dictionary<string, object>? headers, string key)
    {
        try
        {
            if (headers!.TryGetValue(key, out var value) != true)
                return Enumerable.Empty<string>();

            var stringValue = value.ToString();

            return stringValue != null
                ? new[] { stringValue }
                : Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to extract trace context: {ex}", ex);
            return Enumerable.Empty<string>();
        }
    }
}
