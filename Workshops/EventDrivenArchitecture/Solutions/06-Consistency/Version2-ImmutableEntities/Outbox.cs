using System.Text.Json;
using Consistency.Core;

namespace Consistency.Version2_ImmutableEntities;

public record OutboxMessage(Guid OutboxId, string MessageType, string Data)
{
    public static OutboxMessage From(Guid outboxId, object message) =>
        new(outboxId, message.GetType().AssemblyQualifiedName!, JsonSerializer.Serialize(message, message.GetType()));

    public object ToMessage() =>
        JsonSerializer.Deserialize(Data, Type.GetType(MessageType)!)!;
}

public static class Outbox
{
    public static async ValueTask PublishOrStoreInOutbox(
        Database database,
        EventBus eventBus,
        Guid outboxId,
        object[] messages,
        CancellationToken ct
    )
    {
        for (var index = 0; index < messages.Length; index++)
        {
            try
            {
                await eventBus.Publish([messages[index]], ct);
            }
            catch (Core.TimeoutException)
            {
                var notPublished = messages[index..];

                await Retry.UntilSucceeds(token => database.Transaction(async transaction =>
                {
                    foreach (var message in notPublished)
                        await transaction.Store(Guid.NewGuid(), OutboxMessage.From(outboxId, message), token);
                }), ct);
                return;
            }
        }
    }

    public static async ValueTask PublishFromOutbox(
        Database database,
        EventBus eventBus,
        Guid outboxId,
        CancellationToken ct
    )
    {
        var messages = (await database.GetAll<OutboxMessage>(ct))
            .Where(stored => stored.Document.OutboxId == outboxId);

        foreach (var (id, message, _, _) in messages)
        {
            await eventBus.Publish([message.ToMessage()], ct);

            await Retry.UntilSucceeds(token => database.Delete<OutboxMessage>(id, token), ct);
        }
    }
}
