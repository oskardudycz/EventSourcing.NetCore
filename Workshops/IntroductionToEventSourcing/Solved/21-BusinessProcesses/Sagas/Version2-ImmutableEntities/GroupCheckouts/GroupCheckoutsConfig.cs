using BusinessProcesses.Core;
using BusinessProcesses.Sagas.Version2_ImmutableEntities.GuestStayAccounts;

namespace BusinessProcesses.Sagas.Version2_ImmutableEntities.GroupCheckouts;

using static GuestStayAccountEvent;
using static GroupCheckoutCommand;
using static SagaResult;

public static class GroupCheckoutsConfig
{
    public static void ConfigureGroupCheckouts(
        EventStore eventStore,
        GroupCheckOutFacade groupCheckoutFacade
    )
    {
        eventStore
            .Subscribe<GroupCheckoutEvent.GroupCheckoutInitiated>((@event, ct) =>
                eventStore.AppendToStream("commands", GroupCheckoutSaga.Handle(@event).Select(c => c.Message).ToArray<object>(), ct)
            )
            .Subscribe<GuestCheckedOut>((@event, ct) =>
                GroupCheckoutSaga.Handle(@event) is Command<RecordGuestCheckoutCompletion>(var command)
                    ? eventStore.AppendToStream("commands", [command], ct)
                    : ValueTask.CompletedTask
            )
            .Subscribe<GuestCheckOutFailed>((@event, ct) =>
                GroupCheckoutSaga.Handle(@event) is Command<RecordGuestCheckoutFailure>(var command)
                    ? eventStore.AppendToStream("commands", [command], ct)
                    : ValueTask.CompletedTask
            );

        eventStore.Subscribe<RecordGuestCheckoutCompletion>(groupCheckoutFacade.RecordGuestCheckoutCompletion);
        eventStore.Subscribe<RecordGuestCheckoutFailure>(groupCheckoutFacade.RecordGuestCheckoutFailure);
    }
}
