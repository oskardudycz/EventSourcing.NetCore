using Marten;
using Marten.Events;
using Marten.Events.Projections;
using Microsoft.Extensions.Logging;

namespace Core.Marten.Subscriptions;

public class MartenSubscription: IProjection
{
    private readonly IEnumerable<IMartenEventsConsumer> consumers;
    private readonly ILogger<MartenSubscription> logger;

    public MartenSubscription(
        IEnumerable<IMartenEventsConsumer> consumers,
        ILogger<MartenSubscription> logger
    )
    {
        this.consumers = consumers;
        this.logger = logger;
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
        try
        {
            foreach (var consumer in consumers)
            {
                await consumer.ConsumeAsync(operations, streams, ct);
            }
        }
        catch (Exception exc)
        {
            logger.LogError("Error while processing Marten Subscription: {ExceptionMessage}", exc.Message);
            throw;
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
