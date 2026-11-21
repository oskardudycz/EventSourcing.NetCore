using OptimisticConcurrency.Core;
using Database = OptimisticConcurrency.Core.Database;

namespace OptimisticConcurrency.RetryInFacade.Version1_Aggregates.GroupCheckouts;

using static GroupCheckoutCommand;

public class GroupCheckOutFacade(Database database, MessageBus messageBus)
{
    public ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default) =>
        Handle(
            command.GroupCheckoutId,
            groupCheckout => groupCheckout == null
                ? GroupCheckOut.Initiate(
                    command.GroupCheckoutId,
                    command.ClerkId,
                    command.GuestStayIds,
                    command.Now
                )
                : null,
            ct
        );

    public ValueTask RecordGuestCheckoutCompletion(
        RecordGuestCheckoutCompletion command,
        CancellationToken ct = default
    ) =>
        Handle(
            command.GroupCheckoutId,
            groupCheckout =>
            {
                groupCheckout?.RecordGuestCheckoutCompletion(command.GuestStayId, command.CompletedAt);

                return groupCheckout;
            },
            ct
        );

    public ValueTask RecordGuestCheckoutFailure(
        RecordGuestCheckoutFailure command,
        CancellationToken ct = default
    ) =>
        Handle(
            command.GroupCheckoutId,
            groupCheckout =>
            {
                groupCheckout?.RecordGuestCheckoutFailure(command.GuestStayId, command.FailedAt);

                return groupCheckout;
            },
            ct
        );

    private async ValueTask Handle(
        Guid groupCheckoutId,
        Func<GroupCheckOut?, GroupCheckOut?> handle,
        CancellationToken ct
    )
    {
        var events = await Retry.OnConflict<object[]>(async token =>
        {
            var loaded = await database.Get<GroupCheckOut>(groupCheckoutId, token);

            var groupCheckout = handle(loaded?.Document);
            if (groupCheckout == null)
                return [];

            var events = groupCheckout.DequeueUncommittedEvents();
            if (events.Length == 0)
                return events;

            await database.Store(groupCheckoutId, groupCheckout, loaded?.Version ?? 0, token);

            return events;
        }, ct);

        if (events.Length == 0)
            return;

        await messageBus.Publish(events, ct);
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
