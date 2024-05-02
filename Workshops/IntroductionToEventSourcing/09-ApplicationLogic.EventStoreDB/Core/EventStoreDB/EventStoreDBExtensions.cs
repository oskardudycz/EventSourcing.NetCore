using System.Text.Json;
using ApplicationLogic.EventStoreDB.Core.Entities;
using ApplicationLogic.EventStoreDB.Core.Exceptions;
using EventStore.Client;

namespace ApplicationLogic.EventStoreDB.Core.EventStoreDB;

public static class EventStoreDBExtensions
{
    public static Task<T?> AggregateStream<T, TEvent>(
        this EventStoreClient eventStore,
        Func<T> getInitial,
        Guid id,
        CancellationToken ct = default
    ) where T : Aggregate<TEvent> =>
        throw new NotImplementedException("EventStoreDB Extensions not implemented!");

    public static Task<T?> AggregateStream<T, TEvent>(
        this EventStoreClient eventStore,
        Func<T, TEvent, T> evolve,
        Func<T> getInitial,
        Guid id,
        CancellationToken cancellationToken = default
    ) where T : class =>
        throw new NotImplementedException("EventStoreDB Extensions not implemented!");

    public static Task Add<T>(this EventStoreClient eventStore, Guid id, T aggregate, CancellationToken ct)
        where T : class, IAggregate =>
    throw new NotImplementedException("EventStoreDB Extensions not implemented!");

    public static Task Add<T>(this EventStoreClient eventStore, Guid id, object[] events, CancellationToken ct)
        where T : class  =>
        throw new NotImplementedException("EventStoreDB Extensions not implemented!");

    public static Task GetAndUpdate<T, TEvent>(
        this EventStoreClient eventStore,
        Func<T> getInitial,
        Guid id,
        Action<T> handle,
        CancellationToken ct
    )
        where T : Aggregate<TEvent>
        where TEvent: notnull  =>
        throw new NotImplementedException("EventStoreDB Extensions not implemented!");

    public static Task GetAndUpdate<T, TEvent>(
        this EventStoreClient eventStore,
        Func<T, TEvent, T> evolve,
        Func<T> getInitial,
        Guid id,
        Func<T, TEvent[]> handle,
        CancellationToken ct
    )
        where T : class
        where TEvent: notnull =>
        throw new NotImplementedException("EventStoreDB Extensions not implemented!");
}
