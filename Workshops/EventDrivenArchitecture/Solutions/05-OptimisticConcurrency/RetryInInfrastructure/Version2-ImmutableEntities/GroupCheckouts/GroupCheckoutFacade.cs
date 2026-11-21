using OptimisticConcurrency.RetryInInfrastructure.Core;
using OptimisticConcurrency.Core;
using Database = OptimisticConcurrency.Core.Database;

namespace OptimisticConcurrency.RetryInInfrastructure.Version2_ImmutableEntities.GroupCheckouts;

using static GroupCheckoutCommand;

public class GroupCheckoutFacade(Database database, MessageBus messageBus)
{
    private readonly CommandHandler<GroupCheckout, GroupCheckoutEvent> commandHandler =
        new(database, GroupCheckout.Initial, (state, @event) => state.Evolve(@event));

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
        var events = await commandHandler.Handle(groupCheckoutId, handle, ct);

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
