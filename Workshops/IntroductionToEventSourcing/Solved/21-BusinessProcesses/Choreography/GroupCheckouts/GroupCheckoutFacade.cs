using BusinessProcesses.Choreography.GuestStayAccounts;
using BusinessProcesses.Core;

namespace BusinessProcesses.Choreography.GroupCheckouts;

using static GroupCheckoutCommand;
using static GuestStayAccountEvent;
using static GuestStayAccountCommand;

public class GroupCheckOutFacade(EventStore eventStore)
{
    public async ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default)
    {
        var @event = GroupCheckOut.Initiate(command.GroupCheckoutId, command.ClerkId,
            command.GuestStayIds, command.Now);

        await eventStore.AppendToStream(command.GroupCheckoutId.ToString(), [@event], ct);
    }

    public ValueTask GroupCheckoutInitiated(GroupCheckoutEvent.GroupCheckoutInitiated @event,
        CancellationToken ct = default) =>
        eventStore.AppendToStream("commands",
            [
                ..@event.GuestStayIds
                    .Select(guestStayId => new CheckOutGuest(guestStayId, @event.InitiatedAt, @event.GroupCheckoutId))
            ],
            ct
        );

    public async ValueTask GuestCheckedOut(GuestCheckedOut guestCheckedOut, CancellationToken ct = default)
    {
        if (!guestCheckedOut.GroupCheckOutId.HasValue)
            return;

        var groupCheckout = await GetGroupCheckOut(guestCheckedOut.GroupCheckOutId.Value, ct);

        var events =
            groupCheckout.RecordGuestCheckoutCompletion(guestCheckedOut.GuestStayId, guestCheckedOut.CheckedOutAt);

        await eventStore.AppendToStream(groupCheckout.Id.ToString(), [..events], ct);
    }

    public async ValueTask GuestCheckOutFailed(GuestCheckOutFailed checkOutFailed, CancellationToken ct = default)
    {
        if (!checkOutFailed.GroupCheckOutId.HasValue)
            return;

        var groupCheckout = await GetGroupCheckOut(checkOutFailed.GroupCheckOutId.Value, ct);

        var events = groupCheckout.RecordGuestCheckoutFailure(checkOutFailed.GuestStayId, checkOutFailed.FailedAt);

        await eventStore.AppendToStream(groupCheckout.Id.ToString(), [..events], ct);
    }

    private async ValueTask<GroupCheckOut> GetGroupCheckOut(Guid guestStayId, CancellationToken ct)
    {
        var groupCheckout = await eventStore.AggregateStream(
            guestStayId.ToString(),
            (GroupCheckOut state, GroupCheckoutEvent @event) => state.Evolve(@event),
            () => GroupCheckOut.Initial,
            ct
        );

        if (groupCheckout == GroupCheckOut.Initial)
            throw new InvalidOperationException("Entity not found");

        return groupCheckout;
    }
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
