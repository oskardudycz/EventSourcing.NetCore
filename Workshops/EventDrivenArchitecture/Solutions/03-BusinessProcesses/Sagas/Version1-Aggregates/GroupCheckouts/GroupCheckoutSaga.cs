using BusinessProcesses.Core;
using BusinessProcesses.Sagas.Version1_Aggregates.GuestStayAccounts;

namespace BusinessProcesses.Sagas.Version1_Aggregates.GroupCheckouts;

using static GroupCheckoutCommand;
using static GroupCheckoutEvent;
using static GuestStayAccountCommand;
using static GuestStayAccountEvent;

public class GroupCheckoutSaga(MessageBus messageBus)
{
    public ValueTask Handle(GroupCheckoutInitiated @event, CancellationToken ct) =>
        messageBus.Publish(
            @event.GuestStayIds
                .Select(guestAccountId => new CheckOutGuest(guestAccountId, @event.InitiatedAt, @event.GroupCheckoutId))
                .Cast<object>()
                .ToArray(),
            ct
        );

    public ValueTask Handle(GuestCheckedOut @event, CancellationToken ct)
    {
        if (!@event.GroupCheckOutId.HasValue)
            return ValueTask.CompletedTask;

        return messageBus.Publish(
            [
                new RecordGuestCheckoutCompletion(
                    @event.GroupCheckOutId.Value,
                    @event.GuestStayId,
                    @event.CheckedOutAt
                )
            ],
            ct
        );
    }

    public ValueTask Handle(GuestStayAccountEvent.GuestCheckOutFailed @event, CancellationToken ct)
    {
        if (!@event.GroupCheckOutId.HasValue)
            return ValueTask.CompletedTask;

        return messageBus.Publish(
            [
                new RecordGuestCheckoutFailure(
                    @event.GroupCheckOutId.Value,
                    @event.GuestStayId,
                    @event.FailedAt
                )
            ],
            ct
        );
    }
}
