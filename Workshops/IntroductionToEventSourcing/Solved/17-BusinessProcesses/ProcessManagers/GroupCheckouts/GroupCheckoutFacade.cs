using BusinessProcesses.Core;
using BusinessProcesses.ProcessManagers.GuestStayAccounts;
using Database = BusinessProcesses.Core.Database;

namespace BusinessProcesses.ProcessManagers.GroupCheckouts;

using static GroupCheckoutCommand;
using static GuestStayAccountEvent;
using static ProcessManagerResult;

public class GroupCheckOutFacade(Database database, EventStore eventStore, CommandBus commandBus)
{
    public async ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default)
    {
        var messages = GroupCheckOut.Handle(command);

        await ProcessMessages(command.GroupCheckOutId, GroupCheckOut.Initial, messages, ct);
    }

    public async ValueTask GuestCheckedOut(GuestCheckedOut @event, CancellationToken ct = default)
    {
        if (!@event.GroupCheckOutId.HasValue)
            return;

        var groupCheckout = await database.Get<GroupCheckOut>(@event.GroupCheckOutId.Value, ct)
                            ?? throw new InvalidOperationException("Entity not found");

        var messages = groupCheckout.On(@event);

        await ProcessMessages(groupCheckout.Id, groupCheckout, messages, ct);
    }

    public async ValueTask GuestCheckOutFailed(GuestCheckOutFailed @event, CancellationToken ct = default)
    {
        if (!@event.GroupCheckOutId.HasValue)
            return;

        var groupCheckout = await database.Get<GroupCheckOut>(@event.GroupCheckOutId.Value, ct)
                            ?? throw new InvalidOperationException("Entity not found");

        var messages = groupCheckout.On(@event);

        await ProcessMessages(groupCheckout.Id, groupCheckout, messages, ct);
    }

    private async Task ProcessMessages(
        Guid groupCheckOutId,
        GroupCheckOut groupCheckOut,
        ProcessManagerResult[] messages,
        CancellationToken ct
    )
    {
        foreach (var message in messages)
        {
            switch (message)
            {
                case Event (GroupCheckoutEvent @event):
                    await database.Store(groupCheckOutId, groupCheckOut.Evolve(@event), ct);
                    await eventStore.AppendToStream([@event], ct);
                    break;
                case Command(GuestStayAccountCommand command):
                    await commandBus.Send([command], ct);
                    break;
            }
        }
    }
}

public abstract record GroupCheckoutCommand
{
    public record InitiateGroupCheckout(
        Guid GroupCheckOutId,
        Guid ClerkId,
        Guid[] GuestStayIds,
        DateTimeOffset Now
    ): GroupCheckoutCommand;

    private GroupCheckoutCommand() { }
}
