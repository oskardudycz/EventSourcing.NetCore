using OptimisticConcurrency.Core;
using Database = OptimisticConcurrency.Core.Database;

namespace OptimisticConcurrency.Version2_ImmutableEntities.GroupCheckouts;

using static GroupCheckoutCommand;

public class GroupCheckoutFacade(Database database, MessageBus messageBus)
{
    public async ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default)
    {
        if (await database.Get<GroupCheckout>(command.GroupCheckoutId, ct) != null)
            return;
        
        var @event = GroupCheckout.Initiate(
            command.GroupCheckoutId,
            command.ClerkId,
            command.GuestStayAccountIds,
            command.Now
        );

        await database.Store(command.GroupCheckoutId, GroupCheckout.Initial.Evolve(@event), ct);
        await messageBus.Publish([@event], ct);
    }

    public async ValueTask RecordGuestCheckoutCompletion(
        RecordGuestCheckoutCompletion command,
        CancellationToken ct = default
    )
    {
        var groupCheckout = (await database.Get<GroupCheckout>(command.GroupCheckoutId, ct))?.Document;
        if (groupCheckout == null)
            return;

        var events = groupCheckout.RecordGuestCheckoutCompletion(command.GuestStayAccountId, command.Now);

        if (events.Length == 0)
            return;

        await database.Store(command.GroupCheckoutId,
            events.Aggregate(groupCheckout, (state, @event) => state.Evolve(@event)), ct);

        await messageBus.Publish(events.Cast<object>().ToArray(), ct);
    }

    public async ValueTask RecordGuestCheckoutFailure(
        RecordGuestCheckoutFailure command,
        CancellationToken ct = default
    )
    {
        var groupCheckout = (await database.Get<GroupCheckout>(command.GroupCheckoutId, ct))?.Document;
        if (groupCheckout == null)
            return;

        var events = groupCheckout.RecordGuestCheckoutFailure(command.GuestStayAccountId, command.Now);

        if (events.Length == 0)
            return;

        var newState = events.Aggregate(groupCheckout, (state, @event) => state.Evolve(@event));

        await database.Store(command.GroupCheckoutId, newState, ct);

        await messageBus.Publish(events.Cast<object>().ToArray(), ct);
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
