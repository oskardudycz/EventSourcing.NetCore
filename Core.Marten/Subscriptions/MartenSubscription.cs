using Marten;
using Marten.Events;
using Marten.Events.Projections;

namespace Core.Marten.Subscriptions;

public class MartenSubscription: IProjection
{
    private readonly IEnumerable<IMartenEventsConsumer> consumers;

    public MartenSubscription(IEnumerable<IMartenEventsConsumer> consumers)
    {
        this.consumers = consumers;
    }

    public void Apply(
        IDocumentOperations operations,
        IReadOnlyList<StreamAction> streams
    ) =>
        throw new NotImplementedException("Subscriptions should work only in the async scope");

    public async Task ApplyAsync(
        IDocumentOperations operations,
        IReadOnlyList<StreamAction> streams,
        CancellationToken ct
    )
    {
        foreach (var consumer in consumers)
        {
            await consumer.ConsumeAsync(operations, streams, ct);
        }
    }
}


public interface IMartenEventsConsumer
{
    Task ConsumeAsync(
        IDocumentOperations documentOperations,
        IReadOnlyList<StreamAction> streamActions,
        CancellationToken ct
    );
}
