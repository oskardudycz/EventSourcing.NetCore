using Consistency.Core;
using Database = Consistency.Core.Database;

namespace Consistency.Version1_Aggregates.GroupCheckouts;

using static GroupCheckoutCommand;

public class GroupCheckoutFacade
{
    private readonly Database database;
    private readonly MessageBus messageBus;

    public GroupCheckoutFacade(Database database, MessageBus messageBus, BackgroundJobs backgroundJobs)
    {
        this.database = database;
        this.messageBus = messageBus;
    }

    public async ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default)
    {
        var events = await Retry.OnConflict<object[]>(async token =>
        {
            if (await database.Get<GroupCheckout>(command.GroupCheckoutId, token) != null)
                return [];

            var groupCheckout =
                GroupCheckout.Initiate(
                    command.GroupCheckoutId,
                    command.ClerkId,
                    command.GuestStayAccountIds,
                    command.Now
                );

            await database.Store(command.GroupCheckoutId, groupCheckout, 0, token);

            return groupCheckout.DequeueUncommittedEvents();
        }, ct);

        await messageBus.Publish(events, ct);
    }

    public async ValueTask RecordGuestCheckoutCompletion(
        RecordGuestCheckoutCompletion command,
        CancellationToken ct = default
    )
    {
        var events = await Retry.OnConflict(async token =>
        {
            var (groupCheckout, version) = await database.Get<GroupCheckout>(command.GroupCheckoutId, token)
                                           ?? throw new InvalidOperationException("Entity not found");

            groupCheckout.RecordGuestCheckoutCompletion(command.GuestStayAccountId, command.Now);

            await database.Store(command.GroupCheckoutId, groupCheckout, version, token);

            return groupCheckout.DequeueUncommittedEvents();
        }, ct);

        await messageBus.Publish(events, ct);
    }

    public async ValueTask RecordGuestCheckoutFailure(
        RecordGuestCheckoutFailure command,
        CancellationToken ct = default
    )
    {
        var events = await Retry.OnConflict(async token =>
        {
            var (groupCheckout, version) = await database.Get<GroupCheckout>(command.GroupCheckoutId, token)
                                           ?? throw new InvalidOperationException("Entity not found");

            groupCheckout.RecordGuestCheckoutFailure(command.GuestStayAccountId, command.Now);

            await database.Store(command.GroupCheckoutId, groupCheckout, version, token);

            return groupCheckout.DequeueUncommittedEvents();
        }, ct);

        await messageBus.Publish(events, ct);
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
