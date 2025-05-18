﻿using BusinessProcesses.Core;
using BusinessProcesses.ProcessManagers.GuestStayAccounts;

namespace BusinessProcesses.ProcessManagers.GroupCheckouts;

using static GroupCheckoutCommand;
using static GuestStayAccountEvent;
using static ProcessManagerResult;

public class GroupCheckOutFacade(EventStore eventStore, CommandBus commandBus)
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

        var groupCheckout = await  GetGroupCheckOut(@event.GroupCheckOutId.Value, ct);

        var messages = groupCheckout.On(@event);

        await ProcessMessages(groupCheckout.Id, groupCheckout, messages, ct);
    }

    public async ValueTask GuestCheckOutFailed(GuestCheckOutFailed @event, CancellationToken ct = default)
    {
        if (!@event.GroupCheckOutId.HasValue)
            return;

        var groupCheckout = await  GetGroupCheckOut(@event.GroupCheckOutId.Value, ct);

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
                    await eventStore.AppendToStream(groupCheckOutId.ToString(), [@event], ct);
                    break;
                case Command(GuestStayAccountCommand command):
                    await commandBus.Send([command], ct);
                    break;
            }
        }
    }

    private async ValueTask<GroupCheckOut> GetGroupCheckOut(Guid guestStayId, CancellationToken ct)
    {
        var groupCheckout = await eventStore.AggregateStream(
            guestStayId.ToString(),
            (GroupCheckOut state, GroupCheckoutEvent @event) => state.Evolve(@event),
            () => GroupCheckOut.Initial,
            ct
        );

        if(groupCheckout == GroupCheckOut.Initial)
            throw new InvalidOperationException("Entity not found");

        return groupCheckout;
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
