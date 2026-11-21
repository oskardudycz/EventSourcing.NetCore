using OptimisticConcurrency.Core;
using OptimisticConcurrency.RetryInInfrastructure.Version1_Aggregates.GuestStayAccounts;

namespace OptimisticConcurrency.RetryInInfrastructure.Version1_Aggregates.GroupCheckouts;

using static GuestStayAccountEvent;
using static GroupCheckoutCommand;

public static class GroupCheckoutsConfig
{
    public static void ConfigureGroupCheckouts(
        MessageBus messageBus,
        GroupCheckoutFacade groupCheckoutFacade
    )
    {
        var saga = new GroupCheckoutSaga(messageBus);

        messageBus
            .Subscribe<GroupCheckoutEvent.GroupCheckoutInitiated>(saga.Handle)
            .Subscribe<GuestCheckedOut>(saga.Handle)
            .Subscribe<GuestCheckoutFailed>(saga.Handle);

        messageBus.Subscribe<RecordGuestCheckoutCompletion>(groupCheckoutFacade.RecordGuestCheckoutCompletion);
        messageBus.Subscribe<RecordGuestCheckoutFailure>(groupCheckoutFacade.RecordGuestCheckoutFailure);
    }
}
