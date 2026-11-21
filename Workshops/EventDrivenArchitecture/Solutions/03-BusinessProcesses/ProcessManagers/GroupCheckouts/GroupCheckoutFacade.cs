using BusinessProcesses.Core;
using BusinessProcesses.ProcessManagers.GuestStayAccounts;
using Database = BusinessProcesses.Core.Database;

namespace BusinessProcesses.ProcessManagers.GroupCheckouts;

using static GroupCheckoutCommand;
using static GuestStayAccountEvent;
using static ProcessManagerResult;

public class GroupCheckoutFacade(Database database, MessageBus messageBus)
{
    public async ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default)
    {
        var messages = GroupCheckout.Handle(command);

        await ProcessMessages(command.GroupCheckoutId, GroupCheckout.Initial, messages, ct);
    }

    public async ValueTask RecordGuestCheckoutCompletion(GuestCheckedOut @event, CancellationToken ct = default)
    {
        if (!@event.GroupCheckoutId.HasValue)
            return;

        var groupCheckout = await database.Get<GroupCheckout>(@event.GroupCheckoutId.Value, ct);
        if (groupCheckout == null)
            return;

        var messages = groupCheckout.On(@event);

        await ProcessMessages(groupCheckout.Id, groupCheckout, messages, ct);
    }

    public async ValueTask RecordGuestCheckoutFailure(GuestCheckoutFailed @event, CancellationToken ct = default)
    {
        if (!@event.GroupCheckoutId.HasValue)
            return;

        var groupCheckout = await database.Get<GroupCheckout>(@event.GroupCheckoutId.Value, ct);
        if (groupCheckout == null)
            return;

        var messages = groupCheckout.On(@event);

        await ProcessMessages(groupCheckout.Id, groupCheckout, messages, ct);
    }

    private async Task ProcessMessages(
        Guid groupCheckoutId,
        GroupCheckout groupCheckout,
        ProcessManagerResult[] messages,
        CancellationToken ct
    )
    {
        foreach (var message in messages)
        {
            switch (message)
            {
                case Event (GroupCheckoutEvent @event):
                    await database.Store(groupCheckoutId, groupCheckout.Evolve(@event), ct);
                    await messageBus.Publish([@event], ct);
                    break;
                case Command(GuestStayAccountCommand command):
                    await messageBus.Publish([command], ct);
                    break;
            }
        }
    }
}

public abstract record GroupCheckoutCommand
{
    public record InitiateGroupCheckout(
        Guid GroupCheckoutId,
        Guid ClerkId,
        Guid[] GuestStayAccountIds,
        DateTimeOffset Now
    ): GroupCheckoutCommand;

    private GroupCheckoutCommand() { }
}
