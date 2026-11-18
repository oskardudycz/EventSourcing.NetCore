using Idempotency.Core;
using Idempotency.Version2_ImmutableEntities.GuestStayAccounts;

namespace Idempotency.Version2_ImmutableEntities.GroupCheckouts;

using static GuestStayAccountEvent;
using static GroupCheckoutCommand;

public static class GroupCheckoutsConfig
{
    public static void ConfigureGroupCheckouts(
        MessageBus messageBus,
        GroupCheckOutFacade groupCheckoutFacade
    )
    {
        var saga = new GroupCheckoutSaga(messageBus);

        messageBus
            .Subscribe<GroupCheckoutEvent.GroupCheckoutInitiated>(saga.Handle)
            .Subscribe<GuestCheckedOut>(saga.Handle)
            .Subscribe<GuestCheckOutFailed>(saga.Handle);

        messageBus.Subscribe<RecordGuestCheckoutCompletion>(groupCheckoutFacade.RecordGuestCheckoutCompletion);
        messageBus.Subscribe<RecordGuestCheckoutFailure>(groupCheckoutFacade.RecordGuestCheckoutFailure);
    }
}
