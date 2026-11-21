using Consistency.Core;
using Database = Consistency.Core.Database;

namespace Consistency.Version1_Aggregates.GroupCheckouts;

using static GroupCheckoutCommand;

public class GroupCheckoutFacade
{
    private static readonly Guid OutboxId = new("83dcd2c1-28e9-4de1-abce-56466496a0dc");

    private readonly Database database;

    public GroupCheckoutFacade(Database database, MessageBus messageBus, BackgroundJobs backgroundJobs)
    {
        this.database = database;

        backgroundJobs.Register(ct => Outbox.PublishFromOutbox(database, messageBus, OutboxId, ct));
    }

    public async ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default) =>
        await Retry.OnConflict(async token => await Retry.UntilSucceeds(_ => database.Transaction(async transaction =>
        {
            if (await transaction.Get<GroupCheckout>(command.GroupCheckoutId, token) != null)
                return;

            var groupCheckout =
                GroupCheckout.Initiate(
                    command.GroupCheckoutId,
                    command.ClerkId,
                    command.GuestStayAccountIds,
                    command.Now
                );

            await transaction.Store(command.GroupCheckoutId, groupCheckout, 0, token);

            await Outbox.StoreInOutbox(transaction, OutboxId, groupCheckout.DequeueUncommittedEvents(), token);
        }), token), ct);

    public async ValueTask RecordGuestCheckoutCompletion(
        RecordGuestCheckoutCompletion command,
        CancellationToken ct = default
    ) =>
        await Retry.OnConflict(async token => await Retry.UntilSucceeds(_ => database.Transaction(async transaction =>
        {
            var (groupCheckout, version) = await transaction.Get<GroupCheckout>(command.GroupCheckoutId, token)
                                           ?? throw new InvalidOperationException("Entity not found");

            groupCheckout.RecordGuestCheckoutCompletion(command.GuestStayAccountId, command.Now);

            await transaction.Store(command.GroupCheckoutId, groupCheckout, version, token);

            await Outbox.StoreInOutbox(transaction, OutboxId, groupCheckout.DequeueUncommittedEvents(), token);
        }), token), ct);

    public async ValueTask RecordGuestCheckoutFailure(
        RecordGuestCheckoutFailure command,
        CancellationToken ct = default
    ) =>
        await Retry.OnConflict(async token => await Retry.UntilSucceeds(_ => database.Transaction(async transaction =>
        {
            var (groupCheckout, version) = await transaction.Get<GroupCheckout>(command.GroupCheckoutId, token)
                                           ?? throw new InvalidOperationException("Entity not found");

            groupCheckout.RecordGuestCheckoutFailure(command.GuestStayAccountId, command.Now);

            await transaction.Store(command.GroupCheckoutId, groupCheckout, version, token);

            await Outbox.StoreInOutbox(transaction, OutboxId, groupCheckout.DequeueUncommittedEvents(), token);
        }), token), ct);
}

public abstract record GroupCheckoutCommand
{
    public record InitiateGroupCheckout(
        Guid GroupCheckoutId,
        Guid ClerkId,
        Guid[] GuestStayAccountIds,
        DateTimeOffset Now
    ): GroupCheckoutCommand;

    public record RecordGuestCheckoutCompletion(
        Guid GroupCheckoutId,
        Guid GuestStayAccountId,
        DateTimeOffset Now
    ): GroupCheckoutCommand;

    public record RecordGuestCheckoutFailure(
        Guid GroupCheckoutId,
        Guid GuestStayAccountId,
        DateTimeOffset Now
    ): GroupCheckoutCommand;

    private GroupCheckoutCommand() { }
}
