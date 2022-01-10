using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marten.Events;
using Marten.Events.Projections;

namespace Marten.Integration.Tests.Integration;

public class MartenSubscription: IProjection
{
    private readonly IMartenEventsConsumer consumer;

    public MartenSubscription(IMartenEventsConsumer consumer)
    {
        this.consumer = consumer;
    }

    public void Apply(IDocumentOperations operations, IReadOnlyList<StreamAction> streams)
    {
        throw new NotSupportedException("Subscription should be only run asynchronously");
    }

    public Task ApplyAsync(IDocumentOperations operations, IReadOnlyList<StreamAction> streams, CancellationToken ct)
    {
        return consumer.ConsumeAsync(streams);
    }
}

public interface IMartenEventsConsumer
{
    Task ConsumeAsync(IReadOnlyList<StreamAction> streamActions);
}

public class MartenEventsConsumer: IMartenEventsConsumer
{
    public static List<object> Events { get; } = new();

    public Task ConsumeAsync(IReadOnlyList<StreamAction> streamActions)
    {
        foreach (var @event in streamActions.SelectMany(streamAction => streamAction.Events))
        {
            Events.Add(@event);
            Console.WriteLine($"{@event.Sequence} - {@event.EventTypeName}");
        }

        return Task.CompletedTask;
    }
}
