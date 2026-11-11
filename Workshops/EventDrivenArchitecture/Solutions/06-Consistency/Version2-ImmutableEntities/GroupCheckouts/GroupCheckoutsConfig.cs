using Consistency.Core;
using Consistency.Version2_ImmutableEntities.GuestStayAccounts;

namespace Consistency.Version2_ImmutableEntities.GroupCheckouts;

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
                new ValueTask(Retry.UntilSucceeds(token =>
                    messageBus.Publish(GroupCheckoutSaga.Handle(@event).Select(c => c.Message).ToArray(), token), ct))
            )
            .Subscribe<GuestCheckedOut>((@event, ct) =>
                GroupCheckoutSaga.Handle(@event) is Command<RecordGuestCheckoutCompletion>(var command)
                    ? new ValueTask(Retry.UntilSucceeds(token => messageBus.Publish([command], token), ct))
                    : ValueTask.CompletedTask
            )
            .Subscribe<GuestCheckOutFailed>((@event, ct) =>
                GroupCheckoutSaga.Handle(@event) is Command<RecordGuestCheckoutFailure>(var command)
                    ? new ValueTask(Retry.UntilSucceeds(token => messageBus.Publish([command], token), ct))
                    : ValueTask.CompletedTask
            );

        messageBus.Subscribe<RecordGuestCheckoutCompletion>(groupCheckoutFacade.RecordGuestCheckoutCompletion);
        messageBus.Subscribe<RecordGuestCheckoutFailure>(groupCheckoutFacade.RecordGuestCheckoutFailure);
    }
}
