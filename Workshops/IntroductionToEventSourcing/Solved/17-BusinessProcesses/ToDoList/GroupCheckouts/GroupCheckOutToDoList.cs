using BusinessProcesses.ToDoList.GuestStayAccounts;
using BusinessProcesses.ToDoList.GroupCheckouts;
using BusinessProcesses.Core;
using Database = BusinessProcesses.Core.Database;

namespace BusinessProcesses.ToDoList.GroupCheckouts;

using static GroupCheckoutCommand;
using static GroupCheckoutEvent;
using static GuestStayAccountCommand;

public class GroupCheckOutToDoList(Database database, EventStore eventStore, CommandBus commandBus)
{
    public async ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default)
    {
        var groupCheckout = new GroupCheckOut
        {
            Id = command.GroupCheckoutId,
            GuestStayCheckouts = command.GuestStayIds.ToDictionary(id => id, _ => CheckoutStatus.Initiated)
        };

        await database.Store(command.GroupCheckoutId, groupCheckout, ct);

        var @event = new GroupCheckoutInitiated(command.GroupCheckoutId, command.ClerkId,
            command.GuestStayIds, command.Now);

        await eventStore.AppendToStream([@event], ct);
    }

    public ValueTask GroupCheckoutInitiated(GroupCheckoutInitiated @event,
        CancellationToken ct = default) =>
        commandBus.Send(
            [
                ..@event.GuestStayIds
                    .Select(guestStayId => new CheckOutGuest(guestStayId, @event.InitiatedAt, @event.GroupCheckoutId))
            ],
            ct
        );

    public async ValueTask GuestCheckedOut(GuestStayAccountEvent.GuestCheckedOut guestCheckedOut,
        CancellationToken ct = default)
    {
        if (!guestCheckedOut.GroupCheckOutId.HasValue)
            return;

        var groupCheckout = await database.Get<GroupCheckOut>(guestCheckedOut.GroupCheckOutId.Value, ct)
                            ?? throw new InvalidOperationException("Entity not found");

        groupCheckout.GuestStayCheckouts[guestCheckedOut.GuestStayId] = CheckoutStatus.Completed;

        await database.Store(groupCheckout.Id, groupCheckout, ct);

        var completed = TryComplete(groupCheckout, guestCheckedOut.CheckedOutAt);

        if (completed != null)
            await eventStore.AppendToStream([completed], ct);
    }

    public async ValueTask GuestCheckOutFailed(GuestStayAccountEvent.GuestCheckOutFailed checkOutFailed,
        CancellationToken ct = default)
    {
        if (!checkOutFailed.GroupCheckOutId.HasValue)
            return;

        var groupCheckout = await database.Get<GroupCheckOut>(checkOutFailed.GroupCheckOutId.Value, ct)
                            ?? throw new InvalidOperationException("Entity not found");

        groupCheckout.GuestStayCheckouts[checkOutFailed.GuestStayId] = CheckoutStatus.Failed;

        await database.Store(groupCheckout.Id, groupCheckout, ct);

        var completed = TryComplete(groupCheckout, checkOutFailed.FailedAt);

        if (completed != null)
            await eventStore.AppendToStream([completed], ct);
    }

    // This could be also checked by Cron
    private GroupCheckoutEvent? TryComplete(
        GroupCheckOut groupCheckOut,
        DateTimeOffset now
    ) =>
        AreAnyOngoingCheckouts(groupCheckOut.GuestStayCheckouts)
            ? null
            : !AreAnyFailedCheckouts(groupCheckOut.GuestStayCheckouts)
                ? new GroupCheckoutCompleted
                (
                    groupCheckOut.Id,
                    CheckoutsWith(groupCheckOut.GuestStayCheckouts, CheckoutStatus.Completed),
                    now
                )
                : new GroupCheckoutFailed
                (
                    groupCheckOut.Id,
                    CheckoutsWith(groupCheckOut.GuestStayCheckouts, CheckoutStatus.Completed),
                    CheckoutsWith(groupCheckOut.GuestStayCheckouts, CheckoutStatus.Failed),
                    now
                );

    private static bool AreAnyOngoingCheckouts(Dictionary<Guid, CheckoutStatus> guestStayCheckouts) =>
        guestStayCheckouts.Values.Any(status => status is CheckoutStatus.Initiated);

    private static bool AreAnyFailedCheckouts(Dictionary<Guid, CheckoutStatus> guestStayCheckouts) =>
        guestStayCheckouts.Values.Any(status => status is CheckoutStatus.Failed);

    private static Guid[] CheckoutsWith(Dictionary<Guid, CheckoutStatus> guestStayCheckouts, CheckoutStatus status) =>
        guestStayCheckouts
            .Where(pair => pair.Value == status)
            .Select(pair => pair.Key)
            .ToArray();
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
