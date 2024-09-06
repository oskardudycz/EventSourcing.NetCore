using Consistency.Core;
using Database = Consistency.Core.Database;

namespace Consistency.Sagas.Version1_Aggregates.GroupCheckouts;

using static GroupCheckoutCommand;

public class GroupCheckOutFacade(Database database, EventBus eventBus)
{
    public async ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default)
    {
        if (await database.Get<GroupCheckOut>(command.GroupCheckoutId, ct) != null)
            return;

        var groupCheckout =
            GroupCheckOut.Initiate(
                command.GroupCheckoutId,
                command.ClerkId,
                command.GuestStayIds,
                command.Now
            );


        await database.Store(command.GroupCheckoutId, groupCheckout, ct);
        await eventBus.Publish(groupCheckout.DequeueUncommittedEvents(), ct);
    }

    public async ValueTask RecordGuestCheckoutCompletion(
        RecordGuestCheckoutCompletion command,
        CancellationToken ct = default
    )
    {
        var groupCheckout = await database.Get<GroupCheckOut>(command.GroupCheckoutId, ct)
                            ?? throw new InvalidOperationException("Entity not found");

        groupCheckout.RecordGuestCheckoutCompletion(command.GuestStayId, command.CompletedAt);

        await database.Store(command.GroupCheckoutId, groupCheckout, ct);
        await eventBus.Publish(groupCheckout.DequeueUncommittedEvents(), ct);
    }

    public async ValueTask RecordGuestCheckoutFailure(
        RecordGuestCheckoutFailure command,
        CancellationToken ct = default
    )
    {
        var groupCheckout = await database.Get<GroupCheckOut>(command.GroupCheckoutId, ct)
                            ?? throw new InvalidOperationException("Entity not found");

        groupCheckout.RecordGuestCheckoutFailure(command.GuestStayId, command.FailedAt);

        await database.Store(command.GroupCheckoutId, groupCheckout, ct);
        await eventBus.Publish(groupCheckout.DequeueUncommittedEvents(), ct);
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
