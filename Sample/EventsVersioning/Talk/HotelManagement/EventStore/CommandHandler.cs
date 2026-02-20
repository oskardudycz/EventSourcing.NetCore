namespace HotelManagement.EventStore;

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
        var events = await eventStore.ReadStream(id, ct);

        var state = events.Aggregate(getInitial(), evolve);

        var result = handle(state);

        await eventStore.AppendToStream(id, result, ct);
    }
}
