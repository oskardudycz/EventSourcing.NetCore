using OptimisticConcurrency.RetryInInfrastructure.Core;
using OptimisticConcurrency.Core;

namespace OptimisticConcurrency.RetryInInfrastructure.Version1_Aggregates.GuestStayAccounts;

using static GuestStayAccountCommand;

public class GuestStayFacade(Database database, MessageBus messageBus)
{
    private readonly AggregateStore<GuestStayAccount, GuestStayAccountEvent> store = new(database);

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
        var events = await store.Handle(guestStayId, handle, ct);

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
