using Consistency.Core;
using Database = Consistency.Core.Database;

namespace Consistency.Version1_Aggregates.GroupCheckouts;

using static GroupCheckoutCommand;

public class GroupCheckOutFacade
{
    private static readonly Guid OutboxId = new("83dcd2c1-28e9-4de1-abce-56466496a0dc");

    private readonly Database database;

    public GroupCheckOutFacade(Database database, EventBus eventBus, BackgroundJobs backgroundJobs)
    {
        this.database = database;

        backgroundJobs.Register(ct => Outbox.PublishFromOutbox(database, eventBus, OutboxId, ct));
    }

    public async ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default) =>
        await Retry.OnConflict(async token => await Retry.UntilSucceeds(_ => database.Transaction(async transaction =>
        {
            if (await transaction.Get<GroupCheckOut>(command.GroupCheckoutId, token) != null)
                return;

            var groupCheckout =
                GroupCheckOut.Initiate(
                    command.GroupCheckoutId,
                    command.ClerkId,
                    command.GuestStayIds,
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
            var (groupCheckout, version) = await transaction.Get<GroupCheckOut>(command.GroupCheckoutId, token)
                                           ?? throw new InvalidOperationException("Entity not found");

            groupCheckout.RecordGuestCheckoutCompletion(command.GuestStayId, command.CompletedAt);

            await transaction.Store(command.GroupCheckoutId, groupCheckout, version, token);

            await Outbox.StoreInOutbox(transaction, OutboxId, groupCheckout.DequeueUncommittedEvents(), token);
        }), token), ct);

    public async ValueTask RecordGuestCheckoutFailure(
        RecordGuestCheckoutFailure command,
        CancellationToken ct = default
    ) =>
        await Retry.OnConflict(async token => await Retry.UntilSucceeds(_ => database.Transaction(async transaction =>
        {
            var (groupCheckout, version) = await transaction.Get<GroupCheckOut>(command.GroupCheckoutId, token)
                                           ?? throw new InvalidOperationException("Entity not found");

            groupCheckout.RecordGuestCheckoutFailure(command.GuestStayId, command.FailedAt);

            await transaction.Store(command.GroupCheckoutId, groupCheckout, version, token);

            await Outbox.StoreInOutbox(transaction, OutboxId, groupCheckout.DequeueUncommittedEvents(), token);
        }), token), ct);
}

public abstract record GroupCheckoutCommand
{
    public record InitiateGroupCheckout(
        Guid GroupCheckoutId,
        Guid ClerkId,
        Guid[] GuestStayIds,
        DateTimeOffset Now
    ): GroupCheckoutCommand;

    public record RecordGuestCheckoutCompletion(
        Guid GroupCheckoutId,
        Guid GuestStayId,
        DateTimeOffset CompletedAt
    ): GroupCheckoutCommand;

    public record RecordGuestCheckoutFailure(
        Guid GroupCheckoutId,
        Guid GuestStayId,
        DateTimeOffset FailedAt
    ): GroupCheckoutCommand;

    private GroupCheckoutCommand() { }
}
