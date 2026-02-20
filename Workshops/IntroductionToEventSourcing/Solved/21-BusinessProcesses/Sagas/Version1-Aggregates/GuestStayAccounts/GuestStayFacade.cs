using BusinessProcesses.Core;
using BusinessProcesses.Sagas.Version1_Aggregates.GroupCheckouts;

namespace BusinessProcesses.Sagas.Version1_Aggregates.GuestStayAccounts;

using static GuestStayAccountCommand;
using static GroupCheckoutCommand;

public class GuestStayFacade(EventStore eventStore)
{
    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        var account = GuestStayAccount.CheckIn(command.GuestStayId, command.Now);

        await eventStore.AppendToStream(command.GuestStayId.ToString(), account.DequeueUncommittedEvents(), ct);
    }

    public async ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default)
    {
        var account = await GetAccount(command.GuestStayId, ct);

        account.RecordCharge(command.Amount, command.Now);

        await eventStore.AppendToStream(command.GuestStayId.ToString(), account.DequeueUncommittedEvents(), ct);
    }

    public async ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default)
    {
        var account = await GetAccount(command.GuestStayId, ct);

        account.RecordPayment(command.Amount, command.Now);

        await eventStore.AppendToStream(command.GuestStayId.ToString(), account.DequeueUncommittedEvents(), ct);
    }

    public async ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default)
    {
        var account = await GetAccount(command.GuestStayId, ct);

        account.CheckOut(command.Now, command.GroupCheckOutId);

        await eventStore.AppendToStream(command.GuestStayId.ToString(), account.DequeueUncommittedEvents(), ct);
    }

    public ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default) =>
        eventStore.AppendToStream(command.GroupCheckoutId.ToString(), [
            new GroupCheckoutEvent.GroupCheckoutInitiated(
                command.GroupCheckoutId,
                command.ClerkId,
                command.GuestStayIds,
                command.Now
            )
        ], ct);

    private async ValueTask<GuestStayAccount> GetAccount(Guid guestStayId, CancellationToken ct)
    {
        return await eventStore.AggregateStream(
            guestStayId.ToString(),
            (GuestStayAccount? state, GuestStayAccountEvent @event) =>
            {
                state ??= GuestStayAccount.Initial();
                state.Apply(@event);
                return state;
            },
            () => null,
            ct
        ) ?? throw new InvalidOperationException("Entity not found");
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
