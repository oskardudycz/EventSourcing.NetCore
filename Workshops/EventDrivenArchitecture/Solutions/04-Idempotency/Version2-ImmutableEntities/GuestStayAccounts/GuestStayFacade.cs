using Idempotency.Core;

namespace Idempotency.Version2_ImmutableEntities.GuestStayAccounts;

using static GuestStayAccountCommand;

public class GuestStayFacade(Database database, MessageBus messageBus)
{
    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        if (await database.Get<GuestStayAccount>(command.GuestStayId, ct) != null)
            return;

        var @event = GuestStayAccount.CheckIn(command.GuestStayId, command.Now);

        await database.Store(command.GuestStayId, GuestStayAccount.Initial.Evolve(@event), ct);
        await messageBus.Publish([@event], ct);
    }

    public ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default) =>
        Handle(command.GuestStayId, account => account.RecordCharge(command.ChargeId, command.Amount, command.Now), ct);

    public ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default) =>
        Handle(command.GuestStayId, account => account.RecordPayment(command.PaymentId, command.Amount, command.Now), ct);

    public ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default) =>
        Handle(command.GuestStayId, account => account.CheckOut(command.Now, command.GroupCheckOutId), ct);

    private async ValueTask Handle(
        Guid guestStayId,
        Func<GuestStayAccount, GuestStayAccountEvent[]> decide,
        CancellationToken ct
    )
    {
        var account = await database.Get<GuestStayAccount>(guestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        var events = decide(account);

        if (events.Length == 0)
            return;

        await database.Store(guestStayId, events.Aggregate(account, (state, @event) => state.Evolve(@event)), ct);
        await messageBus.Publish(events.Cast<object>().ToArray(), ct);
    }
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
