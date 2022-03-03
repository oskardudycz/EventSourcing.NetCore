using Core.Events;
using Marten;
using Marten.Events;
using Microsoft.Extensions.DependencyInjection;
using IEvent = Core.Events.IEvent;

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
            // TODO: align all handlers to use StreamEvent
            // var streamEvent = new StreamEvent(
            //     @event.Data,
            //     new EventMetadata(
            //         (ulong)@event.Version,
            //         (ulong)@event.Sequence
            //     )
            // );

            using var scope = serviceProvider.CreateScope();
            var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

            if (@event.Data is not IEvent mappedEvent) continue;

            await eventBus.Publish(mappedEvent, ct);
        }
    }
}
