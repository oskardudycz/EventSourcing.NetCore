using System.Threading;
using System.Threading.Tasks;
using Core.Aggregates;
using Core.Events;
using Marten;

namespace Core.Marten.Aggregates;

public static class AggregateExtensions
{
    public static async Task StoreAndPublishEvents(
        this IAggregate aggregate,
        IDocumentSession session,
        IEventBus eventBus,
        CancellationToken cancellationToken = default
    )
    {
        var uncommitedEvents = aggregate.DequeueUncommittedEvents();
        session.Events.Append(aggregate.Id, uncommitedEvents);
        await session.SaveChangesAsync(cancellationToken);
        await eventBus.Publish(uncommitedEvents, cancellationToken);
    }
}
