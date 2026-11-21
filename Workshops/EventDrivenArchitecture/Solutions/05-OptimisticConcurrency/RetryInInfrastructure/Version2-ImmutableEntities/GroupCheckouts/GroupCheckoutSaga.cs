using OptimisticConcurrency.Core;
using OptimisticConcurrency.RetryInInfrastructure.Version2_ImmutableEntities.GuestStayAccounts;

namespace OptimisticConcurrency.RetryInInfrastructure.Version2_ImmutableEntities.GroupCheckouts;

using static GroupCheckoutCommand;
using static GroupCheckoutEvent;
using static GuestStayAccountCommand;
using static GuestStayAccountEvent;

public class GroupCheckoutSaga(MessageBus messageBus)
{
    public ValueTask Handle(GroupCheckoutInitiated @event, CancellationToken ct) =>
        messageBus.Publish(
            @event.GuestStayAccountIds
                .Select(guestAccountId => new CheckOutGuest(guestAccountId, @event.InitiatedAt, @event.GroupCheckoutId))
                .Cast<object>()
                .ToArray(),
            ct
        );

    public ValueTask Handle(GuestCheckedOut @event, CancellationToken ct)
    {
        if (!@event.GroupCheckoutId.HasValue)
            return ValueTask.CompletedTask;

        return messageBus.Publish(
            [
                new RecordGuestCheckoutCompletion(
                    @event.GroupCheckoutId.Value,
                    @event.GuestStayAccountId,
                    @event.CompletedAt
                )
            ],
            ct
        );
    }

    public ValueTask Handle(GuestStayAccountEvent.GuestCheckoutFailed @event, CancellationToken ct)
    {
        if (!@event.GroupCheckoutId.HasValue)
            return ValueTask.CompletedTask;

        return messageBus.Publish(
            [
                new RecordGuestCheckoutFailure(
                    @event.GroupCheckoutId.Value,
                    @event.GuestStayAccountId,
                    @event.FailedAt
                )
            ],
            ct
        );
    }
}
