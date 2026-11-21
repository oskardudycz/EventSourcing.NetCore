using BusinessProcesses.Choreography.GuestStayAccounts;
using BusinessProcesses.Core;
using Database = BusinessProcesses.Core.Database;

namespace BusinessProcesses.Choreography.GroupCheckouts;

using static GroupCheckoutCommand;
using static GuestStayAccountEvent;
using static GuestStayAccountCommand;

public class GroupCheckoutFacade(Database database, MessageBus messageBus)
{
    public async ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default)
    {
        var @event = GroupCheckout.Initiate(command.GroupCheckoutId, command.ClerkId,
            command.GuestStayAccountIds, command.Now);

        await database.Store(command.GroupCheckoutId, GroupCheckout.Initial.Evolve(@event), ct);
        await messageBus.Publish([@event], ct);
    }

    public ValueTask GroupCheckoutInitiated(GroupCheckoutEvent.GroupCheckoutInitiated @event,
        CancellationToken ct = default) =>
        messageBus.Publish(
            [
                ..@event.GuestStayAccountIds
                    .Select(guestStayAccountId => new CheckOutGuest(guestStayAccountId, @event.InitiatedAt, @event.GroupCheckoutId))
            ],
            ct
        );

    public async ValueTask RecordGuestCheckoutCompletion(GuestCheckedOut guestCheckedOut, CancellationToken ct = default)
    {
        if (!guestCheckedOut.GroupCheckoutId.HasValue)
            return;

        var groupCheckout = await database.Get<GroupCheckout>(guestCheckedOut.GroupCheckoutId.Value, ct);
        if (groupCheckout == null)
            return;

        var events =
            groupCheckout.RecordGuestCheckoutCompletion(guestCheckedOut.GuestStayAccountId, guestCheckedOut.CompletedAt);

        await database.Store(groupCheckout.Id,
            events.Aggregate(groupCheckout, (state, @event) => state.Evolve(@event)), ct
        );
        await messageBus.Publish([..events], ct);
    }

    public async ValueTask RecordGuestCheckoutFailure(GuestCheckoutFailed checkOutFailed, CancellationToken ct = default)
    {
        if (!checkOutFailed.GroupCheckoutId.HasValue)
            return;

        var groupCheckout = await database.Get<GroupCheckout>(checkOutFailed.GroupCheckoutId.Value, ct);
        if (groupCheckout == null)
            return;

        var events = groupCheckout.RecordGuestCheckoutFailure(checkOutFailed.GuestStayAccountId, checkOutFailed.FailedAt);

        await database.Store(groupCheckout.Id,
            events.Aggregate(groupCheckout, (state, @event) => state.Evolve(@event)), ct
        );
        await messageBus.Publish([..events], ct);
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
