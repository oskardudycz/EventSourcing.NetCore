using BusinessProcesses.Core;
using BusinessProcesses.Version2_ImmutableEntities.GuestStayAccounts;
using GroupCheckoutEvent = BusinessProcesses.Version1_Aggregates.GroupCheckouts.GroupCheckoutEvent;

namespace BusinessProcesses.Version2_ImmutableEntities;

using static GuestStayAccountCommand;
using static GuestStayAccountEvent;
using static GroupCheckoutCommand;

public class GuestStayFacade(EventStore eventStore)
{
    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        var @event = GuestStayAccount.CheckIn(command.GuestStayId, command.Now);

        await eventStore.AppendToStream(command.GuestStayId.ToString(), [@event], ct);
    }

    public async ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default)
    {
        var account = await GetAccount(command.GuestStayId, ct);

        var @event = account.RecordCharge(command.Amount, command.Now);

        await eventStore.AppendToStream(command.GuestStayId.ToString(), [@event], ct);
    }

    public async ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default)
    {
        var account = await GetAccount(command.GuestStayId, ct);

        var @event = account.RecordPayment(command.Amount, command.Now);

        await eventStore.AppendToStream(command.GuestStayId.ToString(), [@event], ct);
    }

    public async ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default)
    {
        var account = await GetAccount(command.GuestStayId, ct);

        switch (account.CheckOut(command.Now, command.GroupCheckOutId))
        {
            case GuestCheckedOut checkedOut:
            {
                await eventStore.AppendToStream(command.GuestStayId.ToString(), [checkedOut], ct);
                return;
            }
            case GuestCheckOutFailed checkOutFailed:
            {
                await eventStore.AppendToStream(command.GuestStayId.ToString(), [checkOutFailed], ct);
                return;
            }
        }
    }

    public async ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default)
    {
        var @event =
            new GroupCheckoutEvent.GroupCheckoutInitiated(command.GroupCheckoutId, command.ClerkId,
                command.GuestStayIds, command.Now);

        await eventStore.AppendToStream(command.GroupCheckoutId.ToString(), [@event], ct);
    }

    private async ValueTask<GuestStayAccount> GetAccount(Guid guestStayId, CancellationToken ct)
    {
        var account = await eventStore.AggregateStream(
            guestStayId.ToString(),
            (GuestStayAccount state, GuestStayAccountEvent @event) => state.Evolve(@event),
            () => GuestStayAccount.Initial,
            ct
        );
        if (account == GuestStayAccount.Initial)
            throw new InvalidOperationException("Entity not found");

        return account;
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
