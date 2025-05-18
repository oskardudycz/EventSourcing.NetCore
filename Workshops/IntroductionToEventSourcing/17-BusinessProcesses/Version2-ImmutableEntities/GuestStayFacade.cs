using BusinessProcesses.Core;
using BusinessProcesses.Version2_ImmutableEntities.GuestStayAccounts;
using GroupCheckoutEvent = BusinessProcesses.Version1_Aggregates.GroupCheckouts.GroupCheckoutEvent;

namespace BusinessProcesses.Version2_ImmutableEntities;

using static GuestStayAccountCommand;
using static GuestStayAccountEvent;
using static GroupCheckoutCommand;

public class GuestStayFacade(Database database, EventStore eventStore)
{
    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        var @event = GuestStayAccount.CheckIn(command.GuestStayId, command.Now);

        await database.Store(command.GuestStayId, GuestStayAccount.Initial.Evolve(@event), ct);
        await eventStore.AppendToStream([@event], ct);
    }

    public async ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        var @event = account.RecordCharge(command.Amount, command.Now);

        await database.Store(command.GuestStayId, account.Evolve(@event), ct);
        await eventStore.AppendToStream([@event], ct);
    }

    public async ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        var @event = account.RecordPayment(command.Amount, command.Now);

        await database.Store(command.GuestStayId, account.Evolve(@event), ct);
        await eventStore.AppendToStream([@event], ct);
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
                await eventStore.AppendToStream([checkedOut], ct);
                return;
            }
            case GuestCheckOutFailed checkOutFailed:
            {
                await eventStore.AppendToStream([checkOutFailed], ct);
                return;
            }
        }
    }

    public async ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default)
    {
        var @event =
            new GroupCheckoutEvent.GroupCheckoutInitiated(command.GroupCheckoutId, command.ClerkId, command.GuestStayIds, command.Now);

        await eventStore.AppendToStream([@event], ct);
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

public abstract record GroupCheckoutCommand
{
    public record InitiateGroupCheckout(
        Guid GroupCheckoutId,
        Guid ClerkId,
        Guid[] GuestStayIds,
        DateTimeOffset Now
    ): GroupCheckoutCommand;

    private GroupCheckoutCommand() { }
}
