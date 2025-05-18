using BusinessProcesses.Core;

namespace BusinessProcesses.Choreography.GuestStayAccounts;

using static GuestStayAccountCommand;
using static GuestStayAccountEvent;

public class GuestStayFacade(Database database, EventStore eventStore)
{
    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        var @event = GuestStayAccountDecider.CheckIn(command);

        await database.Store(command.GuestStayId, GuestStayAccount.Initial.Evolve(@event), ct);
        await eventStore.AppendToStream([@event], ct);
    }

    public async ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        var @event = GuestStayAccountDecider.RecordCharge(command, account);

        await database.Store(command.GuestStayId, account.Evolve(@event), ct);
        await eventStore.AppendToStream([@event], ct);
    }

    public async ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        var @event = GuestStayAccountDecider.RecordPayment(command, account);

        await database.Store(command.GuestStayId, account.Evolve(@event), ct);
        await eventStore.AppendToStream([@event], ct);
    }

    public async ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        switch (GuestStayAccountDecider.CheckOut(command, account))
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
}
