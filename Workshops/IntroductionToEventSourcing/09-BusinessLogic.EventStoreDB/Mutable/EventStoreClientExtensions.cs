using System.Text.Json;
using EventStore.Client;

namespace IntroductionToEventSourcing.BusinessLogic.Mutable;

public static class EventStoreClientExtensions
{
    public static Task<TAggregate> Get<TAggregate>(
        this EventStoreClient eventStore,
        string streamName,
        CancellationToken cancellationToken = default
    ) where TAggregate : Aggregate
    {
        // Fill logic here.
        throw new NotImplementedException();
    }

    public static Task Add<TAggregate, TCommand>(
        this EventStoreClient eventStore,
        Func<TCommand, string> getStreamName,
        Func<TCommand, TAggregate> action,
        TCommand command,
        CancellationToken cancellationToken = default
    ) where TAggregate : Aggregate
    {
        // Fill logic here.
        throw new NotImplementedException();
    }

    public static Task GetAndUpdate<TAggregate, TCommand>(
        this EventStoreClient eventStore,
        Func<TCommand, string> getStreamName,
        Action<TCommand, TAggregate> action,
        TCommand command,
        CancellationToken cancellationToken = default
    ) where TAggregate : Aggregate
    {
        // Fill logic here.
        throw new NotImplementedException();
    }
}
