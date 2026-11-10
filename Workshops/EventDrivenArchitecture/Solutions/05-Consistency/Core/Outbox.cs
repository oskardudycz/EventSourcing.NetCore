namespace Consistency.Core;

public record OutboxMessage(Guid Id, object Message);

public static class Outbox
{
    public static async ValueTask StoreAndPublish(
        Database database,
        EventBus eventBus,
        object[] messages,
        Func<Database, ValueTask> storeState,
        CancellationToken ct
    )
    {
        var outboxMessages = messages.Select(message => new OutboxMessage(Guid.NewGuid(), message)).ToArray();

        await Retry.UntilSucceeds(_ => database.Transaction(async transaction =>
        {
            await storeState(transaction);

            foreach (var outboxMessage in outboxMessages)
                await transaction.Store(outboxMessage.Id, outboxMessage, ct);
        }), ct);

        foreach (var outboxMessage in outboxMessages)
        {
            await Retry.UntilSucceeds(token => eventBus.Publish([outboxMessage.Message], token), ct);
            await Retry.UntilSucceeds(token => database.Delete<OutboxMessage>(outboxMessage.Id, token), ct);
        }
    }

    public static ValueTask Publish(Database database, EventBus eventBus, object[] messages, CancellationToken ct) =>
        StoreAndPublish(database, eventBus, messages, _ => ValueTask.CompletedTask, ct);
}
