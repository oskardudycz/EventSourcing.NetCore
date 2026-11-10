using Consistency.Core;
using Consistency.Version1_Aggregates.GroupCheckouts;

namespace Consistency.Version1_Aggregates.GuestStayAccounts;

using static GuestStayAccountCommand;
using static GroupCheckoutCommand;

public class GuestStayFacade(Database database, EventBus eventBus)
{
    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        var account = GuestStayAccount.CheckIn(command.GuestStayId, command.Now);

        await Outbox.StoreAndPublish(database, eventBus, account.DequeueUncommittedEvents(),
            transaction => transaction.Store(command.GuestStayId, account, ct), ct);
    }

    public async ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        account.RecordCharge(command.Amount, command.Now);

        await Outbox.StoreAndPublish(database, eventBus, account.DequeueUncommittedEvents(),
            transaction => transaction.Store(command.GuestStayId, account, ct), ct);
    }

    public async ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        account.RecordPayment(command.Amount, command.Now);

        await Outbox.StoreAndPublish(database, eventBus, account.DequeueUncommittedEvents(),
            transaction => transaction.Store(command.GuestStayId, account, ct), ct);
    }

    public async ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        account.CheckOut(command.Now, command.GroupCheckOutId);

        await Outbox.StoreAndPublish(database, eventBus, account.DequeueUncommittedEvents(),
            transaction => transaction.Store(command.GuestStayId, account, ct), ct);
    }

    public ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default) =>
        Outbox.Publish(database, eventBus, [
            new GroupCheckoutEvent.GroupCheckoutInitiated(
                command.GroupCheckoutId,
                command.ClerkId,
                command.GuestStayIds,
                command.Now
            )
        ], ct);
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

