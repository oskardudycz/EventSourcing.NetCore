using Idempotency.Core;
using Idempotency.Version1_Aggregates.GroupCheckouts;

namespace Idempotency.Version1_Aggregates.GuestStayAccounts;

using static GuestStayAccountCommand;
using static GroupCheckoutCommand;

public class GuestStayFacade(Database database, MessageBus messageBus)
{
    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        if (await database.Get<GuestStayAccount>(command.GuestStayId, ct) != null)
            return;

        var account = GuestStayAccount.CheckIn(command.GuestStayId, command.Now);

        await database.Store(command.GuestStayId, account, ct);
        await messageBus.Publish(account.DequeueUncommittedEvents(), ct);
    }

    public async ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        account.RecordCharge(command.ChargeId, command.Amount, command.Now);

        await database.Store(command.GuestStayId, account, ct);
        await messageBus.Publish(account.DequeueUncommittedEvents(), ct);
    }

    public async ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        account.RecordPayment(command.PaymentId, command.Amount, command.Now);

        await database.Store(command.GuestStayId, account, ct);
        await messageBus.Publish(account.DequeueUncommittedEvents(), ct);
    }

    public async ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        account.CheckOut(command.Now, command.GroupCheckOutId);

        await database.Store(command.GuestStayId, account, ct);
        await messageBus.Publish(account.DequeueUncommittedEvents(), ct);
    }

    public ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default) =>
        messageBus.Publish([
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
        Guid ChargeId,
        decimal Amount,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record RecordPayment(
        Guid GuestStayId,
        Guid PaymentId,
        decimal Amount,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record CheckOutGuest(
        Guid GuestStayId,
        DateTimeOffset Now,
        Guid? GroupCheckOutId = null
    ): GuestStayAccountCommand;
}

