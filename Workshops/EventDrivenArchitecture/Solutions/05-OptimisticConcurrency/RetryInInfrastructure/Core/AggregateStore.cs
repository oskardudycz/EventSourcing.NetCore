using OptimisticConcurrency.Core;

namespace OptimisticConcurrency.RetryInInfrastructure.Core;

public class AggregateStore<TAggregate, TEvent>(Database database)
    where TAggregate : Aggregate<TEvent, Guid>
    where TEvent : class
{
    public async ValueTask<object[]> Handle(
        Guid id,
        Func<TAggregate?, TAggregate?> handle,
        CancellationToken ct = default
    ) =>
        await Retry.OnConflict<object[]>(async token =>
        {
            var loaded = await database.Get<TAggregate>(id, token);

            var aggregate = handle(loaded?.Document);
            if (aggregate == null)
                return [];

            var events = aggregate.DequeueUncommittedEvents();
            if (events.Length == 0)
                return events;

            await database.Store(id, aggregate, loaded?.Version ?? 0, token);

            return events;
        }, ct);
}
