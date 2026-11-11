using Idempotency.Core;
using Idempotency.Version1_Aggregates.GuestStayAccounts;

namespace Idempotency.Version1_Aggregates.GroupCheckouts;

using static GuestStayAccountEvent;
using static GroupCheckoutCommand;
using static SagaResult;

public static class GroupCheckoutsConfig
{
    public static void ConfigureGroupCheckouts(
        MessageBus messageBus,
        GroupCheckOutFacade groupCheckoutFacade
    )
    {
        messageBus
            .Subscribe<GroupCheckoutEvent.GroupCheckoutInitiated>((@event, ct) =>
                messageBus.Publish(GroupCheckoutSaga.Handle(@event).Select(c => c.Message).ToArray(), ct)
            )
            .Subscribe<GuestCheckedOut>((@event, ct) =>
                GroupCheckoutSaga.Handle(@event) is Command<RecordGuestCheckoutCompletion>(var command)
                    ? messageBus.Publish([command], ct)
                    : ValueTask.CompletedTask
            )
            .Subscribe<GuestCheckOutFailed>((@event, ct) =>
                GroupCheckoutSaga.Handle(@event) is Command<RecordGuestCheckoutFailure>(var command)
                    ? messageBus.Publish([command], ct)
                    : ValueTask.CompletedTask
            );

        messageBus.Subscribe<RecordGuestCheckoutCompletion>(groupCheckoutFacade.RecordGuestCheckoutCompletion);
        messageBus.Subscribe<RecordGuestCheckoutFailure>(groupCheckoutFacade.RecordGuestCheckoutFailure);
    }
}
