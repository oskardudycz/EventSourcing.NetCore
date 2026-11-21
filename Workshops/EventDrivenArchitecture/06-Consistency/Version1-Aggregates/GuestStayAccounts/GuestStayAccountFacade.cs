using Consistency.Core;
using Consistency.Version1_Aggregates.GroupCheckouts;

namespace Consistency.Version1_Aggregates.GuestStayAccounts;

using static GuestStayAccountCommand;
using static GroupCheckoutCommand;

public class GuestStayAccountFacade
{
    private readonly Database database;
    private readonly MessageBus messageBus;

    public GuestStayAccountFacade(Database database, MessageBus messageBus, BackgroundJobs backgroundJobs)
    {
        this.database = database;
        this.messageBus = messageBus;
    }

    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        var account = GuestStayAccount.CheckIn(command.GuestStayAccountId, command.Now);

        await database.Store(command.GuestStayAccountId, account, 0, ct);
        await messageBus.Publish(account.DequeueUncommittedEvents(), ct);
    }

    public ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default) =>
        Handle(command.GuestStayAccountId, account => account.RecordCharge(command.Amount, command.Now), ct);

    public ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default) =>
        Handle(command.GuestStayAccountId, account => account.RecordPayment(command.Amount, command.Now), ct);

    public ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default) =>
        Handle(command.GuestStayAccountId, account => account.Checkout(command.Now, command.GroupCheckoutId), ct);

    public ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default) =>
        messageBus.Publish([
            new GroupCheckoutEvent.GroupCheckoutInitiated(
                command.GroupCheckoutId,
                command.ClerkId,
                command.GuestStayAccountIds,
                command.Now
            )
        ], ct);

    private async ValueTask Handle(Guid guestStayAccountId, Action<GuestStayAccount> handle, CancellationToken ct)
    {
        var events = await Retry.OnConflict(async token =>
        {
            var (account, version) = await database.Get<GuestStayAccount>(guestStayAccountId, token)
                                     ?? throw new InvalidOperationException("Entity not found");

            handle(account);

            await database.Store(guestStayAccountId, account, version, token);

            return account.DequeueUncommittedEvents();
        }, ct);

        await messageBus.Publish(events, ct);
    }
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

