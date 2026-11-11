using Idempotency.Core;

namespace Idempotency.Version2_ImmutableEntities.GuestStayAccounts;

using static GuestStayAccountCommand;
using static GuestStayAccountEvent;

public class GuestStayFacade(Database database, MessageBus messageBus)
{
    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        var @event = GuestStayAccount.CheckIn(command.GuestStayId, command.Now);

        await database.Store(command.GuestStayId, GuestStayAccount.Initial.Evolve(@event), ct);
        await messageBus.Publish([@event], ct);
    }

    public async ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        var @event = account.RecordCharge(command.ChargeId, command.Amount, command.Now);

        await database.Store(command.GuestStayId, account.Evolve(@event), ct);
        await messageBus.Publish([@event], ct);
    }

    public async ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        var @event = account.RecordPayment(command.PaymentId, command.Amount, command.Now);

        await database.Store(command.GuestStayId, account.Evolve(@event), ct);
        await messageBus.Publish([@event], ct);
    }

    public async ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        switch (account.CheckOut(command.Now, command.GroupCheckOutId))
        {
            case GuestCheckedOut checkedOut:
            {
                await database.Store(command.GuestStayId, account.Evolve(checkedOut), ct);
                await messageBus.Publish([checkedOut], ct);
                return;
            }
            case GuestCheckOutFailed checkOutFailed:
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
