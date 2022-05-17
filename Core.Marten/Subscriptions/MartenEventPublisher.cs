using Core.Events;
using Core.Tracing;
using Core.Tracing.Causation;
using Core.Tracing.Correlation;
using Marten;
using Marten.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Marten.Subscriptions;

public class MartenEventPublisher: IMartenEventsConsumer
{
    private readonly IServiceProvider serviceProvider;

    public MartenEventPublisher(
        IServiceProvider serviceProvider
    )
    {
        this.serviceProvider = serviceProvider;
    }

    public async Task ConsumeAsync(IDocumentOperations documentOperations, IReadOnlyList<StreamAction> streamActions,
        CancellationToken ct)
    {
        foreach (var @event in streamActions.SelectMany(streamAction => streamAction.Events))
        {
            using var scope = serviceProvider.CreateScope();
            var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

            var eventMetadata = new EventMetadata(
                @event.Id.ToString(),
                (ulong)@event.Version,
                (ulong)@event.Sequence,
                new TraceMetadata(
                    @event.CorrelationId != null ? new CorrelationId(@event.CorrelationId) : null,
                    @event.CausationId != null ? new CausationId(@event.CausationId) : null
                )
            );

            await eventBus.Publish(EventEnvelopeFactory.From(@event.Data, eventMetadata), ct);
        }
    }
}
