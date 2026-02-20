using BusinessProcesses.Core;

namespace BusinessProcesses.Choreography.GuestStayAccounts;

using static GuestStayAccountCommand;
using static GuestStayAccountEvent;

public class GuestStayFacade(EventStore eventStore)
{
    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        var @event = GuestStayAccountDecider.CheckIn(command);

        await eventStore.AppendToStream(command.GuestStayId.ToString(), [@event], ct);
    }

    public async ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default)
    {
        var account = await GetAccount(command.GuestStayId, ct);

        var @event = GuestStayAccountDecider.RecordCharge(command, account);

        await eventStore.AppendToStream(command.GuestStayId.ToString(), [@event], ct);
    }

    public async ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default)
    {
        var account = await GetAccount(command.GuestStayId, ct);

        var @event = GuestStayAccountDecider.RecordPayment(command, account);

        await eventStore.AppendToStream(command.GuestStayId.ToString(), [@event], ct);
    }

    public async ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default)
    {
        var account = await GetAccount(command.GuestStayId, ct);

        switch (GuestStayAccountDecider.CheckOut(command, account))
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
