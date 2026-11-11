using OptimisticConcurrency.Core;

namespace OptimisticConcurrency.Version2_ImmutableEntities.GuestStayAccounts;

using static GuestStayAccountCommand;

public class GuestStayFacade(Database database, EventBus eventBus)
{
    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        var events = await Retry.OnConflict<object[]>(async token =>
        {
            if (await database.Get<GuestStayAccount>(command.GuestStayId, token) != null)
                return [];

            var @event = GuestStayAccount.CheckIn(command.GuestStayId, command.Now);

            await database.Store(command.GuestStayId, GuestStayAccount.Initial.Evolve(@event), 0, token);

            return [@event];
        }, ct);

        await eventBus.Publish(events, ct);
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
        var events = await Retry.OnConflict(async token =>
        {
            var (account, version) = await database.Get<GuestStayAccount>(guestStayId, token)
                                     ?? throw new InvalidOperationException("Entity not found");

            var events = decide(account);

            if (events.Length == 0)
                return events;

            await database.Store(guestStayId,
                events.Aggregate(account, (state, @event) => state.Evolve(@event)), version, token);

            return events;
        }, ct);

        if (events.Length == 0)
            return;

        await eventBus.Publish(events.Cast<object>().ToArray(), ct);
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
