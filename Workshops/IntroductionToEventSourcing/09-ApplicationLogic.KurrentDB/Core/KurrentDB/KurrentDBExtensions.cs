using ApplicationLogic.KurrentDB.Core.Entities;
using KurrentDB.Client;

namespace ApplicationLogic.KurrentDB.Core.KurrentDB;

public static class KurrentDBExtensions
{
    public static Task<T?> AggregateStream<T, TEvent>(
        this KurrentDBClient eventStore,
        Func<T> getInitial,
        Guid id,
        CancellationToken ct = default
    ) where T : Aggregate<TEvent> =>
        throw new NotImplementedException("KurrentDB Extensions not implemented!");

    public static Task<T?> AggregateStream<T, TEvent>(
        this KurrentDBClient eventStore,
        Func<T, TEvent, T> evolve,
        Func<T> getInitial,
        Guid id,
        CancellationToken cancellationToken = default
    ) where T : class =>
        throw new NotImplementedException("KurrentDB Extensions not implemented!");

    public static Task Add<T>(this KurrentDBClient eventStore, Guid id, T aggregate, CancellationToken ct)
        where T : class, IAggregate =>
    throw new NotImplementedException("KurrentDB Extensions not implemented!");

    public static Task Add<T>(this KurrentDBClient eventStore, Guid id, object[] events, CancellationToken ct)
        where T : class  =>
        throw new NotImplementedException("KurrentDB Extensions not implemented!");

    public static Task GetAndUpdate<T, TEvent>(
        this KurrentDBClient eventStore,
        Func<T> getInitial,
        Guid id,
        Action<T> handle,
        CancellationToken ct
    )
        where T : Aggregate<TEvent>
        where TEvent: notnull  =>
        throw new NotImplementedException("KurrentDB Extensions not implemented!");

    public static Task GetAndUpdate<T, TEvent>(
        this KurrentDBClient eventStore,
        Func<T, TEvent, T> evolve,
        Func<T> getInitial,
        Guid id,
        Func<T, TEvent[]> handle,
        CancellationToken ct
    )
        where T : class
        where TEvent: notnull =>
        throw new NotImplementedException("KurrentDB Extensions not implemented!");
}
