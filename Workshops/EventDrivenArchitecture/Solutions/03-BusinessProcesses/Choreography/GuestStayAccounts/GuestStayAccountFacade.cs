using BusinessProcesses.Core;

namespace BusinessProcesses.Choreography.GuestStayAccounts;

using static GuestStayAccountCommand;
using static GuestStayAccountEvent;

public class GuestStayAccountFacade(Database database, MessageBus messageBus)
{
    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        var @event = GuestStayAccountDecider.CheckIn(command);

        await database.Store(command.GuestStayAccountId, GuestStayAccount.Initial.Evolve(@event), ct);
        await messageBus.Publish([@event], ct);
    }

    public async ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayAccountId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        var @event = GuestStayAccountDecider.RecordCharge(command, account);

        await database.Store(command.GuestStayAccountId, account.Evolve(@event), ct);
        await messageBus.Publish([@event], ct);
    }

    public async ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayAccountId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        var @event = GuestStayAccountDecider.RecordPayment(command, account);

        await database.Store(command.GuestStayAccountId, account.Evolve(@event), ct);
        await messageBus.Publish([@event], ct);
    }

    public async ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayAccountId, ct);
        if (account == null)
            return;

        switch (GuestStayAccountDecider.Checkout(command, account))
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
