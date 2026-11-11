using Consistency.Core;
using Database = Consistency.Core.Database;

namespace Consistency.Version1_Aggregates.GroupCheckouts;

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

            var groupCheckout =
                GroupCheckOut.Initiate(
                    command.GroupCheckoutId,
                    command.ClerkId,
                    command.GuestStayIds,
                    command.Now
                );

            await database.Store(command.GroupCheckoutId, groupCheckout, 0, token);

            return groupCheckout.DequeueUncommittedEvents();
        }, ct);

        await eventBus.Publish(events, ct);
    }

    public async ValueTask RecordGuestCheckoutCompletion(
        RecordGuestCheckoutCompletion command,
        CancellationToken ct = default
    )
    {
        var events = await Retry.OnConflict(async token =>
        {
            var (groupCheckout, version) = await database.Get<GroupCheckOut>(command.GroupCheckoutId, token)
                                           ?? throw new InvalidOperationException("Entity not found");

            groupCheckout.RecordGuestCheckoutCompletion(command.GuestStayId, command.CompletedAt);

            await database.Store(command.GroupCheckoutId, groupCheckout, version, token);

            return groupCheckout.DequeueUncommittedEvents();
        }, ct);

        await eventBus.Publish(events, ct);
    }

    public async ValueTask RecordGuestCheckoutFailure(
        RecordGuestCheckoutFailure command,
        CancellationToken ct = default
    )
    {
        var events = await Retry.OnConflict(async token =>
        {
            var (groupCheckout, version) = await database.Get<GroupCheckOut>(command.GroupCheckoutId, token)
                                           ?? throw new InvalidOperationException("Entity not found");

            groupCheckout.RecordGuestCheckoutFailure(command.GuestStayId, command.FailedAt);

            await database.Store(command.GroupCheckoutId, groupCheckout, version, token);

            return groupCheckout.DequeueUncommittedEvents();
        }, ct);

        await eventBus.Publish(events, ct);
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
