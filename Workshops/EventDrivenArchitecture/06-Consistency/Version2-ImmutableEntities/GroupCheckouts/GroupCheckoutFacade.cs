using Consistency.Core;
using Database = Consistency.Core.Database;

namespace Consistency.Version2_ImmutableEntities.GroupCheckouts;

using static GroupCheckoutCommand;

public class GroupCheckOutFacade
{
    private readonly Database database;
    private readonly EventBus eventBus;

    public GroupCheckOutFacade(Database database, EventBus eventBus, BackgroundJobs backgroundJobs)
    {
        this.database = database;
        this.eventBus = eventBus;
    }

    public async ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default)
    {
        var events = await Retry.OnConflict<object[]>(async token =>
        {
            if (await database.Get<GroupCheckOut>(command.GroupCheckoutId, token) != null)
                return [];

            var @event = GroupCheckOut.Initiate(
                command.GroupCheckoutId,
                command.ClerkId,
                command.GuestStayIds,
                command.Now
            );

            await database.Store(command.GroupCheckoutId, GroupCheckOut.Initial.Evolve(@event), 0, token);

            return [@event];
        }, ct);

        await eventBus.Publish(events, ct);
    }

    public ValueTask RecordGuestCheckoutCompletion(
        RecordGuestCheckoutCompletion command,
        CancellationToken ct = default
    ) =>
        Handle(
            command.GroupCheckoutId,
            groupCheckout => groupCheckout.RecordGuestCheckoutCompletion(command.GuestStayId, command.CompletedAt),
            ct
        );

    public ValueTask RecordGuestCheckoutFailure(
        RecordGuestCheckoutFailure command,
        CancellationToken ct = default
    ) =>
        Handle(
            command.GroupCheckoutId,
            groupCheckout => groupCheckout.RecordGuestCheckoutFailure(command.GuestStayId, command.FailedAt),
            ct
        );

    private async ValueTask Handle(
        Guid groupCheckoutId,
        Func<GroupCheckOut, GroupCheckoutEvent[]> decide,
        CancellationToken ct
    )
    {
        var events = await Retry.OnConflict(async token =>
        {
            var (groupCheckout, version) = await database.Get<GroupCheckOut>(groupCheckoutId, token)
                                           ?? throw new InvalidOperationException("Entity not found");

            var events = decide(groupCheckout);

            if (events.Length == 0)
                return events;

            await database.Store(groupCheckoutId,
                events.Aggregate(groupCheckout, (state, @event) => state.Evolve(@event)), version, token);

            return events;
        }, ct);

        if (events.Length == 0)
            return;

        await eventBus.Publish(events.Cast<object>().ToArray(), ct);
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
