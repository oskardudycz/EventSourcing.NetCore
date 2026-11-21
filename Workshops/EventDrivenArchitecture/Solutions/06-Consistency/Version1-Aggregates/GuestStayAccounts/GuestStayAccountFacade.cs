using Consistency.Core;
using Consistency.Version1_Aggregates.GroupCheckouts;

namespace Consistency.Version1_Aggregates.GuestStayAccounts;

using static GuestStayAccountCommand;
using static GroupCheckoutCommand;

public class GuestStayAccountFacade
{
    private static readonly Guid OutboxId = new("71c26a37-98f2-4147-9c4b-0172a6a525fe");

    private readonly Database database;

    public GuestStayAccountFacade(Database database, MessageBus messageBus, BackgroundJobs backgroundJobs)
    {
        this.database = database;

        backgroundJobs.Register(ct => Outbox.PublishFromOutbox(database, messageBus, OutboxId, ct));
    }

    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default) =>
        await Retry.UntilSucceeds(token => database.Transaction(async transaction =>
        {
            var account = GuestStayAccount.CheckIn(command.GuestStayAccountId, command.Now);

            await transaction.Store(command.GuestStayAccountId, account, 0, token);

            await Outbox.StoreInOutbox(transaction, OutboxId, account.DequeueUncommittedEvents(), token);
        }), ct);

    public ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default) =>
        Handle(command.GuestStayAccountId, account => account.RecordCharge(command.Amount, command.Now), ct);

    public ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default) =>
        Handle(command.GuestStayAccountId, account => account.RecordPayment(command.Amount, command.Now), ct);

    public ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default) =>
        Handle(command.GuestStayAccountId, account => account.Checkout(command.Now, command.GroupCheckoutId), ct);

    public async ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default) =>
        await Retry.UntilSucceeds(token => database.Transaction(transaction =>
            Outbox.StoreInOutbox(transaction, OutboxId, [
                new GroupCheckoutEvent.GroupCheckoutInitiated(
                    command.GroupCheckoutId,
                    command.ClerkId,
                    command.GuestStayAccountIds,
                    command.Now
                )
            ], token)
        ), ct);

    private async ValueTask Handle(Guid guestStayAccountId, Action<GuestStayAccount> handle, CancellationToken ct) =>
        await Retry.OnConflict(async token => await Retry.UntilSucceeds(_ => database.Transaction(async transaction =>
        {
            var (account, version) = await transaction.Get<GuestStayAccount>(guestStayAccountId, token)
                                     ?? throw new InvalidOperationException("Entity not found");

            handle(account);

            await transaction.Store(guestStayAccountId, account, version, token);

            await Outbox.StoreInOutbox(transaction, OutboxId, account.DequeueUncommittedEvents(), token);
        }), token), ct);
}

public abstract record GuestStayAccountCommand
{
    public record CheckInGuest(
        Guid GuestStayAccountId,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record RecordCharge(
        Guid GuestStayAccountId,
        decimal Amount,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record RecordPayment(
        Guid GuestStayAccountId,
        decimal Amount,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record CheckOutGuest(
        Guid GuestStayAccountId,
        DateTimeOffset Now,
        Guid? GroupCheckoutId = null
    ): GuestStayAccountCommand;
}

