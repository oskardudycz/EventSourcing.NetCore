using OptimisticConcurrency.Core;

namespace OptimisticConcurrency.RetryInFacade.Version2_ImmutableEntities.GuestStayAccounts;

using static GuestStayAccountCommand;

public class GuestStayAccountFacade(Database database, MessageBus messageBus)
{
    public ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default) =>
        Handle(
            command.GuestStayAccountId,
            account => account.Status == GuestStayAccountStatus.NotExisting
                ? [GuestStayAccount.CheckIn(command.GuestStayAccountId, command.Now)]
                : [],
            ct
        );

    public ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default) =>
        Handle(
            command.GuestStayAccountId,
            account =>
            {
                if (account.Status == GuestStayAccountStatus.NotExisting)
                    throw new InvalidOperationException("Entity not found");

                return account.RecordCharge(command.ChargeId, command.Amount, command.Now);
            },
            ct
        );

    public ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default) =>
        Handle(
            command.GuestStayAccountId,
            account =>
            {
                if (account.Status == GuestStayAccountStatus.NotExisting)
                    throw new InvalidOperationException("Entity not found");

                return account.RecordPayment(command.PaymentId, command.Amount, command.Now);
            },
            ct
        );

    public ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default) =>
        Handle(
            command.GuestStayAccountId,
            account => account.Status == GuestStayAccountStatus.NotExisting
                ? []
                : account.Checkout(command.Now, command.GroupCheckoutId),
            ct
        );

    private async ValueTask Handle(
        Guid guestStayAccountId,
        Func<GuestStayAccount, GuestStayAccountEvent[]> handle,
        CancellationToken ct
    )
    {
        var events = await Retry.OnConflict(async token =>
        {
            var loaded = await database.Get<GuestStayAccount>(guestStayAccountId, token);
            var account = loaded?.Document ?? GuestStayAccount.Initial;

            var events = handle(account);
            if (events.Length == 0)
                return events;

            await database.Store(guestStayAccountId,
                events.Aggregate(account, (state, @event) => state.Evolve(@event)), loaded?.Version ?? 0, token);

            return events;
        }, ct);

        if (events.Length == 0)
            return;

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
