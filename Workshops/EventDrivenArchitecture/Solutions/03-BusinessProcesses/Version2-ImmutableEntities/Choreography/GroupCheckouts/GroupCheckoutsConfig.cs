using BusinessProcesses.Core;
using BusinessProcesses.Version2_ImmutableEntities.Choreography.GuestStayAccounts;

namespace BusinessProcesses.Version2_ImmutableEntities.Choreography.GroupCheckouts;

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
