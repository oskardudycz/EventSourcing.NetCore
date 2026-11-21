using OptimisticConcurrency.RetryInInfrastructure.Core;
using OptimisticConcurrency.Core;
using Database = OptimisticConcurrency.Core.Database;

namespace OptimisticConcurrency.RetryInInfrastructure.Version1_Aggregates.GroupCheckouts;

using static GroupCheckoutCommand;

public class GroupCheckOutFacade(Database database, MessageBus messageBus)
{
    private readonly AggregateStore<GroupCheckOut, GroupCheckoutEvent> store = new(database);

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
        var events = await store.Handle(groupCheckoutId, handle, ct);

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
