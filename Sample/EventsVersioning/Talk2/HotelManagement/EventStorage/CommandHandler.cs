namespace HotelManagement.EventStorage;

public class CommandHandler<T>(
    Func<T, object, T> evolve,
    Func<T> getInitial
)
{
    public async Task Handle(
        IEventStore eventStore,
        string id,
        Func<T, object[]> handle,
        CancellationToken ct
    )
    {
        // 1. Read events from the stream
        var events = await eventStore.ReadStream(id, ct);

        // 2. Build state from events
        var state = events.Aggregate(getInitial(), evolve);

        // 3. Run business logic
        var result = handle(state);

        // 4. Store new events
        if (result.Length > 0)
            await eventStore.AppendToStream(id, result, ct);
    }
}
