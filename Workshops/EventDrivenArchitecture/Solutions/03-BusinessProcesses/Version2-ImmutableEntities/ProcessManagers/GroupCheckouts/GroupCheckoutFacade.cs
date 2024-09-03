using BusinessProcesses.Core;
using BusinessProcesses.Version2_ImmutableEntities.ProcessManagers.GuestStayAccounts;
using Database = BusinessProcesses.Core.Database;

namespace BusinessProcesses.Version2_ImmutableEntities.ProcessManagers.GroupCheckouts;

using static GroupCheckoutCommand;
using static GuestStayAccountEvent;
using static ProcessManagerResult;

public class GroupCheckoutFacade(Database database, EventBus eventBus, CommandBus commandBus)
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

    public async ValueTask GuestCheckOutFailed(GuestCheckoutFailed @event, CancellationToken ct = default)
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
                    await eventBus.Publish([@event], ct);
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
