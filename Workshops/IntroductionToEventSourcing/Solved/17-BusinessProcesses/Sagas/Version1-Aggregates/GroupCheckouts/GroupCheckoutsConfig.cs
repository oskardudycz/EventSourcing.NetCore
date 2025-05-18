using BusinessProcesses.Core;
using BusinessProcesses.Sagas.Version1_Aggregates.GuestStayAccounts;

namespace BusinessProcesses.Sagas.Version1_Aggregates.GroupCheckouts;

using static GuestStayAccountEvent;
using static GroupCheckoutCommand;
using static SagaResult;

public static class GroupCheckoutsConfig
{
    public static void ConfigureGroupCheckouts(
        EventStore eventStore,
        CommandBus commandBus,
        GroupCheckOutFacade groupCheckoutFacade
    )
    {
        eventStore
            .Subscribe<GroupCheckoutEvent.GroupCheckoutInitiated>((@event, ct) =>
                commandBus.Send(GroupCheckoutSaga.Handle(@event).Select(c => c.Message).ToArray(), ct)
            )
            .Subscribe<GuestCheckedOut>((@event, ct) =>
                GroupCheckoutSaga.Handle(@event) is Command<RecordGuestCheckoutCompletion>(var command)
                    ? commandBus.Send([command], ct)
                    : ValueTask.CompletedTask
            )
            .Subscribe<GuestCheckOutFailed>((@event, ct) =>
                GroupCheckoutSaga.Handle(@event) is Command<RecordGuestCheckoutFailure>(var command)
                    ? commandBus.Send([command], ct)
                    : ValueTask.CompletedTask
            );

        commandBus.Handle<RecordGuestCheckoutCompletion>(groupCheckoutFacade.RecordGuestCheckoutCompletion);
        commandBus.Handle<RecordGuestCheckoutFailure>(groupCheckoutFacade.RecordGuestCheckoutFailure);
    }
}
