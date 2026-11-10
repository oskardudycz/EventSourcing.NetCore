using Consistency.Core;
using Database = Consistency.Core.Database;

namespace Consistency.Version2_ImmutableEntities.GroupCheckouts;

using static GroupCheckoutCommand;

public class GroupCheckOutFacade(Database database, EventBus eventBus)
{
    public async ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default)
    {
        if (await database.Get<GroupCheckOut>(command.GroupCheckoutId, ct) != null)
            return;

        var @event = GroupCheckOut.Initiate(
            command.GroupCheckoutId,
            command.ClerkId,
            command.GuestStayIds,
            command.Now
        );

        await Outbox.StoreAndPublish(database, eventBus, [@event],
            transaction => transaction.Store(command.GroupCheckoutId, GroupCheckOut.Initial.Evolve(@event), ct), ct);
    }

    public async ValueTask RecordGuestCheckoutCompletion(
        RecordGuestCheckoutCompletion command,
        CancellationToken ct = default
    )
    {
        var groupCheckout = await database.Get<GroupCheckOut>(command.GroupCheckoutId, ct)
                            ?? throw new InvalidOperationException("Entity not found");

        var events = groupCheckout.RecordGuestCheckoutCompletion(command.GuestStayId, command.CompletedAt);

        if (events.Length == 0)
            return;

        await Outbox.StoreAndPublish(database, eventBus, events.Cast<object>().ToArray(),
            transaction => transaction.Store(command.GroupCheckoutId,
            events.Aggregate(groupCheckout, (state, @event) => state.Evolve(@event)), ct), ct);
    }

    public async ValueTask RecordGuestCheckoutFailure(
        RecordGuestCheckoutFailure command,
        CancellationToken ct = default
    )
    {
        var groupCheckout = await database.Get<GroupCheckOut>(command.GroupCheckoutId, ct)
                            ?? throw new InvalidOperationException("Entity not found");

        var events = groupCheckout.RecordGuestCheckoutFailure(command.GuestStayId, command.FailedAt);

        if (events.Length == 0)
            return;

        var newState = events.Aggregate(groupCheckout, (state, @event) => state.Evolve(@event));

        await Outbox.StoreAndPublish(database, eventBus, events.Cast<object>().ToArray(),
            transaction => transaction.Store(command.GroupCheckoutId, newState, ct), ct);
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
