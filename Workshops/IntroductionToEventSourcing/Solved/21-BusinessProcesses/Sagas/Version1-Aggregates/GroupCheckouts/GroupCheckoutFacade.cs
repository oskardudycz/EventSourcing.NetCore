using BusinessProcesses.Core;

namespace BusinessProcesses.Sagas.Version1_Aggregates.GroupCheckouts;

using static GroupCheckoutCommand;

public class GroupCheckOutFacade(EventStore eventStore)
{
    public async ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default)
    {
        var groupCheckout =
            GroupCheckOut.Initiate(
                command.GroupCheckoutId,
                command.ClerkId,
                command.GuestStayIds,
                command.Now
            );

        await eventStore.AppendToStream(command.GroupCheckoutId.ToString(), groupCheckout.DequeueUncommittedEvents(),
            ct);
    }

    public async ValueTask RecordGuestCheckoutCompletion(
        RecordGuestCheckoutCompletion command,
        CancellationToken ct = default
    )
    {
        var groupCheckout = await GetGroupCheckOut(command.GroupCheckoutId, ct);

        groupCheckout.RecordGuestCheckoutCompletion(command.GuestStayId, command.CompletedAt);

        await eventStore.AppendToStream(command.GroupCheckoutId.ToString(), groupCheckout.DequeueUncommittedEvents(),
            ct);
    }

    public async ValueTask RecordGuestCheckoutFailure(
        RecordGuestCheckoutFailure command,
        CancellationToken ct = default
    )
    {
        var groupCheckout = await GetGroupCheckOut(command.GroupCheckoutId, ct);

        groupCheckout.RecordGuestCheckoutFailure(command.GuestStayId, command.FailedAt);

        await eventStore.AppendToStream(command.GroupCheckoutId.ToString(), groupCheckout.DequeueUncommittedEvents(),
            ct);
    }

    private async ValueTask<GroupCheckOut> GetGroupCheckOut(Guid guestStayId, CancellationToken ct)
    {
        return await eventStore.AggregateStream(
            guestStayId.ToString(),
            (GroupCheckOut? state, GroupCheckoutEvent @event) =>
            {
                state ??= GroupCheckOut.Initial();
                state.Apply(@event);
                return state;
            },
            () => null,
            ct
        ) ?? throw new InvalidOperationException("Entity not found");
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

    public record RecordGuestCheckoutCompletion(
        Guid GroupCheckoutId,
        Guid GuestStayId,
        DateTimeOffset CompletedAt
    ): GroupCheckoutCommand;

    public record RecordGuestCheckoutFailure(
        Guid GroupCheckoutId,
        Guid GuestStayId,
        DateTimeOffset FailedAt
    ): GroupCheckoutCommand;

    private GroupCheckoutCommand() { }
}
