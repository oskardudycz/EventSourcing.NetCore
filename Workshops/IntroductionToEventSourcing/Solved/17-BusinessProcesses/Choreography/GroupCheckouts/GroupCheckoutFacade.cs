using BusinessProcesses.Choreography.GuestStayAccounts;
using BusinessProcesses.Core;
using Database = BusinessProcesses.Core.Database;

namespace BusinessProcesses.Choreography.GroupCheckouts;

using static GroupCheckoutCommand;
using static GuestStayAccountEvent;
using static GuestStayAccountCommand;

public class GroupCheckOutFacade(Database database, EventStore eventStore, CommandBus commandBus)
{
    public async ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default)
    {
        var @event = GroupCheckOut.Initiate(command.GroupCheckoutId, command.ClerkId,
            command.GuestStayIds, command.Now);

        await database.Store(command.GroupCheckoutId, GroupCheckOut.Initial.Evolve(@event), ct);
        await eventStore.AppendToStream([@event], ct);
    }

    public ValueTask GroupCheckoutInitiated(GroupCheckoutEvent.GroupCheckoutInitiated @event,
        CancellationToken ct = default) =>
        commandBus.Send(
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

        var groupCheckout = await database.Get<GroupCheckOut>(guestCheckedOut.GroupCheckOutId.Value, ct)
                            ?? throw new InvalidOperationException("Entity not found");

        var events =
            groupCheckout.RecordGuestCheckoutCompletion(guestCheckedOut.GuestStayId, guestCheckedOut.CheckedOutAt);

        await database.Store(groupCheckout.Id,
            events.Aggregate(groupCheckout, (state, @event) => state.Evolve(@event)), ct
        );
        await eventStore.AppendToStream([..events], ct);
    }

    public async ValueTask GuestCheckOutFailed(GuestCheckOutFailed checkOutFailed, CancellationToken ct = default)
    {
        if (!checkOutFailed.GroupCheckOutId.HasValue)
            return;

        var groupCheckout = await database.Get<GroupCheckOut>(checkOutFailed.GroupCheckOutId.Value, ct)
                            ?? throw new InvalidOperationException("Entity not found");

        var events = groupCheckout.RecordGuestCheckoutFailure(checkOutFailed.GuestStayId, checkOutFailed.FailedAt);

        await database.Store(groupCheckout.Id,
            events.Aggregate(groupCheckout, (state, @event) => state.Evolve(@event)), ct
        );
        await eventStore.AppendToStream([..events], ct);
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
