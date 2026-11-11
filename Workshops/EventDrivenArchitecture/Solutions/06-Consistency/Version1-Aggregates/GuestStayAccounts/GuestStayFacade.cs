using Consistency.Core;
using Consistency.Version1_Aggregates.GroupCheckouts;

namespace Consistency.Version1_Aggregates.GuestStayAccounts;

using static GuestStayAccountCommand;
using static GroupCheckoutCommand;

public class GuestStayFacade
{
    private static readonly Guid OutboxId = new("71c26a37-98f2-4147-9c4b-0172a6a525fe");

    private readonly Database database;

    public GuestStayFacade(Database database, MessageBus messageBus, BackgroundJobs backgroundJobs)
    {
        this.database = database;

        backgroundJobs.Register(ct => Outbox.PublishFromOutbox(database, messageBus, OutboxId, ct));
    }

    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default) =>
        await Retry.UntilSucceeds(token => database.Transaction(async transaction =>
        {
            var account = GuestStayAccount.CheckIn(command.GuestStayId, command.Now);

            await transaction.Store(command.GuestStayId, account, 0, token);

            await Outbox.StoreInOutbox(transaction, OutboxId, account.DequeueUncommittedEvents(), token);
        }), ct);

    public ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default) =>
        Handle(command.GuestStayId, account => account.RecordCharge(command.Amount, command.Now), ct);

    public ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default) =>
        Handle(command.GuestStayId, account => account.RecordPayment(command.Amount, command.Now), ct);

    public ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default) =>
        Handle(command.GuestStayId, account => account.CheckOut(command.Now, command.GroupCheckOutId), ct);

    public async ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default) =>
        await Retry.UntilSucceeds(token => database.Transaction(transaction =>
            Outbox.StoreInOutbox(transaction, OutboxId, [
                new GroupCheckoutEvent.GroupCheckoutInitiated(
                    command.GroupCheckoutId,
                    command.ClerkId,
                    command.GuestStayIds,
                    command.Now
                )
            ], token)
        ), ct);

    private async ValueTask Handle(Guid guestStayId, Action<GuestStayAccount> handle, CancellationToken ct) =>
        await Retry.OnConflict(async token => await Retry.UntilSucceeds(_ => database.Transaction(async transaction =>
        {
            var (account, version) = await transaction.Get<GuestStayAccount>(guestStayId, token)
                                     ?? throw new InvalidOperationException("Entity not found");

            handle(account);

            await transaction.Store(guestStayId, account, version, token);

            await Outbox.StoreInOutbox(transaction, OutboxId, account.DequeueUncommittedEvents(), token);
        }), token), ct);
}

public abstract record GuestStayAccountCommand
{
    public record CheckInGuest(
        Guid GuestStayId,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record RecordCharge(
        Guid GuestStayId,
        decimal Amount,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record RecordPayment(
        Guid GuestStayId,
        decimal Amount,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record CheckOutGuest(
        Guid GuestStayId,
        DateTimeOffset Now,
        Guid? GroupCheckOutId = null
    ): GuestStayAccountCommand;
}

