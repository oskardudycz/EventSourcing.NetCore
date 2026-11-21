using OptimisticConcurrency.Core;
using Database = OptimisticConcurrency.Core.Database;

namespace OptimisticConcurrency.RetryInFacade.Version2_ImmutableEntities.GroupCheckouts;

using static GroupCheckoutCommand;

public class GroupCheckoutFacade(Database database, MessageBus messageBus)
{
    public ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default) =>
        Handle(
            command.GroupCheckoutId,
            groupCheckout => groupCheckout.Status == CheckoutStatus.NotExisting
                ? [GroupCheckout.Initiate(
                    command.GroupCheckoutId,
                    command.ClerkId,
                    command.GuestStayAccountIds,
                    command.Now
                )]
                : [],
            ct
        );

    public ValueTask RecordGuestCheckoutCompletion(
        RecordGuestCheckoutCompletion command,
        CancellationToken ct = default
    ) =>
        Handle(
            command.GroupCheckoutId,
            groupCheckout => groupCheckout.Status == CheckoutStatus.NotExisting
                ? []
                : groupCheckout.RecordGuestCheckoutCompletion(command.GuestStayAccountId, command.Now),
            ct
        );

    public ValueTask RecordGuestCheckoutFailure(
        RecordGuestCheckoutFailure command,
        CancellationToken ct = default
    ) =>
        Handle(
            command.GroupCheckoutId,
            groupCheckout => groupCheckout.Status == CheckoutStatus.NotExisting
                ? []
                : groupCheckout.RecordGuestCheckoutFailure(command.GuestStayAccountId, command.Now),
            ct
        );

    private async ValueTask Handle(
        Guid groupCheckoutId,
        Func<GroupCheckout, GroupCheckoutEvent[]> handle,
        CancellationToken ct
    )
    {
        var events = await Retry.OnConflict(async token =>
        {
            var loaded = await database.Get<GroupCheckout>(groupCheckoutId, token);
            var groupCheckout = loaded?.Document ?? GroupCheckout.Initial;

            var events = handle(groupCheckout);
            if (events.Length == 0)
                return events;

            await database.Store(groupCheckoutId,
                events.Aggregate(groupCheckout, (state, @event) => state.Evolve(@event)), loaded?.Version ?? 0, token);

            return events;
        }, ct);

        if (events.Length == 0)
            return;

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
