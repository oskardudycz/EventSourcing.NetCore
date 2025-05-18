using BusinessProcesses.Core;

namespace BusinessProcesses.Sagas.Version2_ImmutableEntities.GuestStayAccounts;

using static GuestStayAccountCommand;
using static GuestStayAccountEvent;

public class GuestStayFacade(Database database, EventBus eventBus)
{
    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        var @event = GuestStayAccount.CheckIn(command.GuestStayId, command.Now);

        await database.Store(command.GuestStayId, GuestStayAccount.Initial.Evolve(@event), ct);
        await eventBus.Publish([@event], ct);
    }

    public async ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        var @event = account.RecordCharge(command.Amount, command.Now);

        await database.Store(command.GuestStayId, account.Evolve(@event), ct);
        await eventBus.Publish([@event], ct);
    }

    public async ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        var @event = account.RecordPayment(command.Amount, command.Now);

        await database.Store(command.GuestStayId, account.Evolve(@event), ct);
        await eventBus.Publish([@event], ct);
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
                await eventBus.Publish([checkedOut], ct);
                return;
            }
            case GuestCheckOutFailed checkOutFailed:
            {
                await eventBus.Publish([checkOutFailed], ct);
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
        decimal Amount,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record RecordPayment(
        Guid GuestStayId,
        decimal Amount,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record CheckOutGuest(
        Guid GuestStayId,
        DateTimeOffset Now,
        Guid? GroupCheckOutId = null
    ): GuestStayAccountCommand;
}
