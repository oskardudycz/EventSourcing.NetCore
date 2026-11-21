using OptimisticConcurrency.Core;

namespace OptimisticConcurrency.RetryInFacade.Version1_Aggregates.GuestStayAccounts;

using static GuestStayAccountCommand;

public class GuestStayFacade(Database database, MessageBus messageBus)
{
    public ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default) =>
        Handle(
            command.GuestStayId,
            account => account == null ? GuestStayAccount.CheckIn(command.GuestStayId, command.Now) : null,
            ct
        );

    public ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default) =>
        Handle(
            command.GuestStayId,
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
            command.GuestStayId,
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
            command.GuestStayId,
            account =>
            {
                account?.CheckOut(command.Now, command.GroupCheckOutId);

                return account;
            },
            ct
        );

    private async ValueTask Handle(
        Guid guestStayId,
        Func<GuestStayAccount?, GuestStayAccount?> handle,
        CancellationToken ct
    )
    {
        var events = await Retry.OnConflict<object[]>(async token =>
        {
            var loaded = await database.Get<GuestStayAccount>(guestStayId, token);

            var account = handle(loaded?.Document);
            if (account == null)
                return [];

            var events = account.DequeueUncommittedEvents();
            if (events.Length == 0)
                return events;

            await database.Store(guestStayId, account, loaded?.Version ?? 0, token);

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
