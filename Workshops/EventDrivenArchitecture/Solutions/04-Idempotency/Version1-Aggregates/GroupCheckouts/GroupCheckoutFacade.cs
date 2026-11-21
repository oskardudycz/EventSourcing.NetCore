using Idempotency.Core;
using Database = Idempotency.Core.Database;

namespace Idempotency.Version1_Aggregates.GroupCheckouts;

using static GroupCheckoutCommand;

public class GroupCheckoutFacade(Database database, MessageBus messageBus)
{
    public async ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default)
    {
        if (await database.Get<GroupCheckout>(command.GroupCheckoutId, ct) != null)
            return;

        var groupCheckout =
            GroupCheckout.Initiate(
                command.GroupCheckoutId,
                command.ClerkId,
                command.GuestStayAccountIds,
                command.Now
            );


        await database.Store(command.GroupCheckoutId, groupCheckout, ct);
        await messageBus.Publish(groupCheckout.DequeueUncommittedEvents(), ct);
    }

    public async ValueTask RecordGuestCheckoutCompletion(
        RecordGuestCheckoutCompletion command,
        CancellationToken ct = default
    )
    {
        var groupCheckout = await database.Get<GroupCheckout>(command.GroupCheckoutId, ct);
        if (groupCheckout == null)
            return;

        groupCheckout.RecordGuestCheckoutCompletion(command.GuestStayAccountId, command.Now);

        await database.Store(command.GroupCheckoutId, groupCheckout, ct);
        await messageBus.Publish(groupCheckout.DequeueUncommittedEvents(), ct);
    }

    public async ValueTask RecordGuestCheckoutFailure(
        RecordGuestCheckoutFailure command,
        CancellationToken ct = default
    )
    {
        var groupCheckout = await database.Get<GroupCheckout>(command.GroupCheckoutId, ct);
        if (groupCheckout == null)
            return;

        groupCheckout.RecordGuestCheckoutFailure(command.GuestStayAccountId, command.Now);

        await database.Store(command.GroupCheckoutId, groupCheckout, ct);
        await messageBus.Publish(groupCheckout.DequeueUncommittedEvents(), ct);
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

    public record RecordGuestCheckoutCompletion(
        Guid GroupCheckoutId,
        Guid GuestStayAccountId,
        DateTimeOffset Now
    ): GroupCheckoutCommand;

    public record RecordGuestCheckoutFailure(
        Guid GroupCheckoutId,
        Guid GuestStayAccountId,
        DateTimeOffset Now
    ): GroupCheckoutCommand;

    private GroupCheckoutCommand() { }
}
