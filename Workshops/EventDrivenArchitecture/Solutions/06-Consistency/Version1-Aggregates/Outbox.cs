using System.Text.Json;
using Consistency.Core;

namespace Consistency.Version1_Aggregates;

public record OutboxMessage(Guid OutboxId, string MessageType, string Data)
{
    public static OutboxMessage From(Guid outboxId, object message) =>
        new(outboxId, message.GetType().AssemblyQualifiedName!, JsonSerializer.Serialize(message, message.GetType()));

    public object ToMessage() =>
        JsonSerializer.Deserialize(Data, Type.GetType(MessageType)!)!;
}

public record OutboxCheckpoint(long Position);

public static class Outbox
{
    public static async ValueTask StoreInOutbox(
        Database transaction,
        Guid outboxId,
        object[] messages,
        CancellationToken ct
    )
    {
        foreach (var message in messages)
            await transaction.Store(Guid.NewGuid(), OutboxMessage.From(outboxId, message), ct);
    }

    public static async ValueTask PublishFromOutbox(
        Database database,
        EventBus eventBus,
        Guid outboxId,
        CancellationToken ct
    )
    {
        var checkpoint = (await database.Get<OutboxCheckpoint>(outboxId, ct))?.Document.Position ?? 0;

        var messages = (await database.GetAll<OutboxMessage>(ct))
            .Where(stored => stored.Document.OutboxId == outboxId && stored.Position > checkpoint);

        foreach (var (_, message, _, position) in messages)
        {
            await eventBus.Publish([message.ToMessage()], ct);

            await Retry.UntilSucceeds(token => database.Store(outboxId, new OutboxCheckpoint(position), token), ct);
        }
    }
}
