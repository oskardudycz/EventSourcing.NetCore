using BusinessProcesses.Core;

namespace BusinessProcesses.Sagas.Version2_ImmutableEntities.GroupCheckouts;

using static GroupCheckoutCommand;

public class GroupCheckOutFacade(EventStore eventStore)
{
    public async ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default)
    {
        var @event = GroupCheckOut.Initiate(
            command.GroupCheckoutId,
            command.ClerkId,
            command.GuestStayIds,
            command.Now
        );

        await eventStore.AppendToStream(command.GroupCheckoutId.ToString(),[@event], ct);
    }

    public async ValueTask RecordGuestCheckoutCompletion(
        RecordGuestCheckoutCompletion command,
        CancellationToken ct = default
    )
    {
        var groupCheckout = await GetGroupCheckout(command.GroupCheckoutId, ct);

        var events = groupCheckout.RecordGuestCheckoutCompletion(command.GuestStayId, command.CompletedAt);

        await eventStore.AppendToStream(command.GroupCheckoutId.ToString(),events.Cast<object>().ToArray(), ct);
    }

    public async ValueTask RecordGuestCheckoutFailure(
        RecordGuestCheckoutFailure command,
        CancellationToken ct = default
    )
    {
        var groupCheckout = await GetGroupCheckout(command.GroupCheckoutId, ct);

        var events = groupCheckout.RecordGuestCheckoutFailure(command.GuestStayId, command.FailedAt);

        await eventStore.AppendToStream(command.GroupCheckoutId.ToString(),events.Cast<object>().ToArray(), ct);
    }

    private async ValueTask<GroupCheckOut> GetGroupCheckout(Guid guestStayId, CancellationToken ct)
    {
        var account = await eventStore.AggregateStream(
            guestStayId.ToString(),
            (GroupCheckOut state, GroupCheckoutEvent @event) => state.Evolve(@event),
            () => GroupCheckOut.Initial,
            ct
        );
        if (account == GroupCheckOut.Initial)
            throw new InvalidOperationException("Entity not found");

        return account;
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
