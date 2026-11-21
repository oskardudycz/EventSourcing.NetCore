using BusinessProcesses.Choreography.GuestStayAccounts;
using BusinessProcesses.Core;

namespace BusinessProcesses.Choreography.GroupCheckouts;

using static GuestStayAccountEvent;

public static class GroupCheckoutsConfig
{
    public static void ConfigureGroupCheckouts(
        MessageBus messageBus,
        GroupCheckoutFacade groupCheckoutFacade
    )
    {
        messageBus
            .Subscribe<GroupCheckoutEvent.GroupCheckoutInitiated>(groupCheckoutFacade.GroupCheckoutInitiated)
            .Subscribe<GuestCheckedOut>(groupCheckoutFacade.RecordGuestCheckoutCompletion)
            .Subscribe<GuestCheckoutFailed>(groupCheckoutFacade.RecordGuestCheckoutFailure);
    }
}
