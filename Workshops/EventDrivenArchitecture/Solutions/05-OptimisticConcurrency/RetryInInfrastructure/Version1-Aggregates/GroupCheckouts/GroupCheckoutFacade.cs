using OptimisticConcurrency.RetryInInfrastructure.Core;
using OptimisticConcurrency.Core;
using Database = OptimisticConcurrency.Core.Database;

namespace OptimisticConcurrency.RetryInInfrastructure.Version1_Aggregates.GroupCheckouts;

using static GroupCheckoutCommand;

public class GroupCheckoutFacade(Database database, MessageBus messageBus)
{
    private readonly AggregateStore<GroupCheckout, GroupCheckoutEvent> store = new(database);

    public ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default) =>
        Handle(
            command.GroupCheckoutId,
            groupCheckout => groupCheckout == null
                ? GroupCheckout.Initiate(
                    command.GroupCheckoutId,
                    command.ClerkId,
                    command.GuestStayAccountIds,
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
                groupCheckout?.RecordGuestCheckoutCompletion(command.GuestStayAccountId, command.Now);

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
                groupCheckout?.RecordGuestCheckoutFailure(command.GuestStayAccountId, command.Now);

                return groupCheckout;
            },
            ct
        );

    private async ValueTask Handle(
        Guid groupCheckoutId,
        Func<GroupCheckout?, GroupCheckout?> handle,
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
