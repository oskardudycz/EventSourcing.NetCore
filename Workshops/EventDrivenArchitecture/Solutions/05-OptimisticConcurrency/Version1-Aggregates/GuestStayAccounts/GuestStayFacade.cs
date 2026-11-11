using OptimisticConcurrency.Core;
using OptimisticConcurrency.Version1_Aggregates.GroupCheckouts;

namespace OptimisticConcurrency.Version1_Aggregates.GuestStayAccounts;

using static GuestStayAccountCommand;
using static GroupCheckoutCommand;

public class GuestStayFacade(Database database, MessageBus messageBus)
{
    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        var events = await Retry.OnConflict<object[]>(async token =>
        {
            if (await database.Get<GuestStayAccount>(command.GuestStayId, token) != null)
                return [];

            var account = GuestStayAccount.CheckIn(command.GuestStayId, command.Now);

            await database.Store(command.GuestStayId, account, 0, token);

            return account.DequeueUncommittedEvents();
        }, ct);

        await messageBus.Publish(events, ct);
    }

    public ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default) =>
        Handle(command.GuestStayId, account => account.RecordCharge(command.ChargeId, command.Amount, command.Now), ct);

    public ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default) =>
        Handle(command.GuestStayId, account => account.RecordPayment(command.PaymentId, command.Amount, command.Now), ct);

    public ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default) =>
        Handle(command.GuestStayId, account => account.CheckOut(command.Now, command.GroupCheckOutId), ct);

    public ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default) =>
        messageBus.Publish([
            new GroupCheckoutEvent.GroupCheckoutInitiated(
                command.GroupCheckoutId,
                command.ClerkId,
                command.GuestStayIds,
                command.Now
            )
        ], ct);

    private async ValueTask Handle(Guid guestStayId, Action<GuestStayAccount> handle, CancellationToken ct)
    {
        var events = await Retry.OnConflict(async token =>
        {
            var (account, version) = await database.Get<GuestStayAccount>(guestStayId, token)
                                     ?? throw new InvalidOperationException("Entity not found");

            handle(account);

            await database.Store(guestStayId, account, version, token);

            return account.DequeueUncommittedEvents();
        }, ct);

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

