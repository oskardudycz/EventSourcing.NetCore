using Idempotency.Core;

namespace Idempotency.Version2_ImmutableEntities.GuestStayAccounts;

using static GuestStayAccountCommand;
using static GuestStayAccountEvent;

public class GuestStayAccountFacade(Database database, MessageBus messageBus)
{
    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        var @event = GuestStayAccount.CheckIn(command.GuestStayAccountId, command.Now);

        await database.Store(command.GuestStayAccountId, GuestStayAccount.Initial.Evolve(@event), ct);
        await messageBus.Publish([@event], ct);
    }

    public async ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayAccountId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        var @event = account.RecordCharge(command.ChargeId, command.Amount, command.Now);

        await database.Store(command.GuestStayAccountId, account.Evolve(@event), ct);
        await messageBus.Publish([@event], ct);
    }

    public async ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayAccountId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        var @event = account.RecordPayment(command.PaymentId, command.Amount, command.Now);

        await database.Store(command.GuestStayAccountId, account.Evolve(@event), ct);
        await messageBus.Publish([@event], ct);
    }

    public async ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayAccountId, ct);
        if (account == null)
            return;

        switch (account.Checkout(command.Now, command.GroupCheckoutId))
        {
            case GuestCheckedOut checkedOut:
            {
                await database.Store(command.GuestStayAccountId, account.Evolve(checkedOut), ct);
                await messageBus.Publish([checkedOut], ct);
                return;
            }
            case GuestCheckoutFailed checkOutFailed:
            {
                await messageBus.Publish([checkOutFailed], ct);
                return;
            }
        }
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
