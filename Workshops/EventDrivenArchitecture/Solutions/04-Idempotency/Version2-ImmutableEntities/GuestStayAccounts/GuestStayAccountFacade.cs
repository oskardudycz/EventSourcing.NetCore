using Idempotency.Core;

namespace Idempotency.Version2_ImmutableEntities.GuestStayAccounts;

using static GuestStayAccountCommand;

public class GuestStayAccountFacade(Database database, MessageBus messageBus)
{
    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        if (await database.Get<GuestStayAccount>(command.GuestStayAccountId, ct) != null)
            return;

        var @event = GuestStayAccount.CheckIn(command.GuestStayAccountId, command.Now);

        await database.Store(command.GuestStayAccountId, GuestStayAccount.Initial.Evolve(@event), ct);
        await messageBus.Publish([@event], ct);
    }

    public ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default) =>
        Handle(command.GuestStayAccountId, account => account.RecordCharge(command.ChargeId, command.Amount, command.Now), ct);

    public ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default) =>
        Handle(command.GuestStayAccountId, account => account.RecordPayment(command.PaymentId, command.Amount, command.Now), ct);

    public async ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayAccountId, ct);
        if (account == null)
            return;

        var events = account.Checkout(command.Now, command.GroupCheckoutId);

        if (events.Length == 0)
            return;

        await database.Store(command.GuestStayAccountId, events.Aggregate(account, (state, @event) => state.Evolve(@event)), ct);
        await messageBus.Publish(events.Cast<object>().ToArray(), ct);
    }

    private async ValueTask Handle(
        Guid guestStayAccountId,
        Func<GuestStayAccount, GuestStayAccountEvent[]> decide,
        CancellationToken ct
    )
    {
        var account = await database.Get<GuestStayAccount>(guestStayAccountId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        var events = decide(account);

        if (events.Length == 0)
            return;

        await database.Store(guestStayAccountId, events.Aggregate(account, (state, @event) => state.Evolve(@event)), ct);
        await messageBus.Publish(events.Cast<object>().ToArray(), ct);
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
        Guid ChargeId,
        decimal Amount,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record RecordPayment(
        Guid GuestStayAccountId,
        Guid PaymentId,
        decimal Amount,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record CheckOutGuest(
        Guid GuestStayAccountId,
        DateTimeOffset Now,
        Guid? GroupCheckoutId = null
    ): GuestStayAccountCommand;
}
