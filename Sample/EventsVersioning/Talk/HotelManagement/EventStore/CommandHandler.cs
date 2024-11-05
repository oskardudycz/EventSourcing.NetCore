namespace HotelManagement.EventStore;

public class CommandHandler<T, TEvent>(
    IEventStore eventStore,
    Func<T, TEvent, T> evolve,
    Func<T> getInitial
) where TEvent : notnull
{
    public async Task GetAndUpdate(
        Guid id,
        Func<T, TEvent[]> handle,
        CancellationToken ct
    )
    {
        var events = await eventStore.ReadStream<TEvent>(id, ct);

        var state = events.Aggregate(getInitial(), evolve);

        var result = handle(state);

        if(result.Length > 0)
            await eventStore.AppendToStream(id, result.Cast<object>(), ct);
    }
}
