using OptimisticConcurrency.Core;

namespace OptimisticConcurrency.RetryInInfrastructure.Core;

public class CommandHandler<TState, TEvent>(Database database, TState initialState, Func<TState, TEvent, TState> evolve)
    where TState : class
{
    public async ValueTask<TEvent[]> Handle(
        Guid id,
        Func<TState, TEvent[]> handle,
        CancellationToken ct = default
    ) =>
        await Retry.OnConflict(async token =>
        {
            var loaded = await database.Get<TState>(id, token);
            var state = loaded?.Document ?? initialState;

            var events = handle(state);
            if (events.Length == 0)
                return events;

            await database.Store(id, events.Aggregate(state, evolve), loaded?.Version ?? 0, token);

            return events;
        }, ct);
}
