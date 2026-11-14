using Core.Events;
using Core.KurrentDB.Serialization;
using Core.Projections;
using KurrentDB.Client;

namespace Core.KurrentDB.Events;

public static class AggregateStreamExtensions
{
    public static async Task<T?> AggregateStream<T>(
        this KurrentDBClient eventStore,
        Guid id,
        CancellationToken cancellationToken,
        ulong? fromVersion = null
    ) where T : class, IProjection
    {
        var readResult = eventStore.ReadStreamAsync(
            Direction.Forwards,
            StreamNameMapper.ToStreamId<T>(id),
            fromVersion ?? StreamPosition.Start,
            cancellationToken: cancellationToken
        );

        if (await readResult.ReadState.ConfigureAwait(false) == ReadState.StreamNotFound)
            return null;

        var aggregate = (T)Activator.CreateInstance(typeof(T), true)!;

        await foreach (var @event in readResult)
        {
            var eventData = @event.Deserialize();

            aggregate.Apply(eventData!);
        }

        return aggregate;
    }
}
