using BusinessProcesses.Choreography.GuestStayAccounts;
using BusinessProcesses.Core;

namespace BusinessProcesses.Choreography.GroupCheckouts;

using static GuestStayAccountEvent;

public static class GroupCheckoutsConfig
{
    public static void ConfigureGroupCheckouts(
        EventBus eventBus,
        GroupCheckOutFacade groupCheckoutFacade
    )
    {
        eventBus
            .Subscribe<GroupCheckoutEvent.GroupCheckoutInitiated>(groupCheckoutFacade.GroupCheckoutInitiated)
            .Subscribe<GuestCheckedOut>(groupCheckoutFacade.GuestCheckedOut)
            .Subscribe<GuestCheckOutFailed>(groupCheckoutFacade.GuestCheckOutFailed);
    }
}
