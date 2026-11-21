using OptimisticConcurrency.RetryInInfrastructure.Core;
using OptimisticConcurrency.Core;
using Database = OptimisticConcurrency.Core.Database;

namespace OptimisticConcurrency.RetryInInfrastructure.Version2_ImmutableEntities.GroupCheckouts;

using static GroupCheckoutCommand;

public class GroupCheckOutFacade(Database database, MessageBus messageBus)
{
    private readonly CommandHandler<GroupCheckOut, GroupCheckoutEvent> commandHandler =
        new(database, GroupCheckOut.Initial, (state, @event) => state.Evolve(@event));

    public ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default) =>
        Handle(
            command.GroupCheckoutId,
            groupCheckout => groupCheckout.Status == CheckoutStatus.NotExisting
                ? [GroupCheckOut.Initiate(
                    command.GroupCheckoutId,
                    command.ClerkId,
                    command.GuestStayIds,
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
                : groupCheckout.RecordGuestCheckoutCompletion(command.GuestStayId, command.CompletedAt),
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
                : groupCheckout.RecordGuestCheckoutFailure(command.GuestStayId, command.FailedAt),
            ct
        );

    private async ValueTask Handle(
        Guid groupCheckoutId,
        Func<GroupCheckOut, GroupCheckoutEvent[]> handle,
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
