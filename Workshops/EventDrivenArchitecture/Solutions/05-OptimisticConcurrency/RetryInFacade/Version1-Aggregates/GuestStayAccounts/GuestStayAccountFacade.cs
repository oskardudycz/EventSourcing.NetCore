using OptimisticConcurrency.Core;

namespace OptimisticConcurrency.RetryInFacade.Version1_Aggregates.GuestStayAccounts;

using static GuestStayAccountCommand;

public class GuestStayAccountFacade(Database database, MessageBus messageBus)
{
    public ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default) =>
        Handle(
            command.GuestStayAccountId,
            account => account == null ? GuestStayAccount.CheckIn(command.GuestStayAccountId, command.Now) : null,
            ct
        );

    public ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default) =>
        Handle(
            command.GuestStayAccountId,
            account =>
            {
                if (account == null)
                    throw new InvalidOperationException("Entity not found");

                account.RecordCharge(command.ChargeId, command.Amount, command.Now);

                return account;
            },
            ct
        );

    public ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default) =>
        Handle(
            command.GuestStayAccountId,
            account =>
            {
                if (account == null)
                    throw new InvalidOperationException("Entity not found");

                account.RecordPayment(command.PaymentId, command.Amount, command.Now);

                return account;
            },
            ct
        );

    public ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default) =>
        Handle(
            command.GuestStayAccountId,
            account =>
            {
                account?.Checkout(command.Now, command.GroupCheckoutId);

                return account;
            },
            ct
        );

    private async ValueTask Handle(
        Guid guestStayAccountId,
        Func<GuestStayAccount?, GuestStayAccount?> handle,
        CancellationToken ct
    )
    {
        var events = await Retry.OnConflict<object[]>(async token =>
        {
            var loaded = await database.Get<GuestStayAccount>(guestStayAccountId, token);

            var account = handle(loaded?.Document);
            if (account == null)
                return [];

            var events = account.DequeueUncommittedEvents();
            if (events.Length == 0)
                return events;

            await database.Store(guestStayAccountId, account, loaded?.Version ?? 0, token);

            return events;
        }, ct);

        if (events.Length == 0)
            return;

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
