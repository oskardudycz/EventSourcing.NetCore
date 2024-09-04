using BusinessProcesses.Core;
using BusinessProcesses.ProcessManagers.GuestStayAccounts;

namespace BusinessProcesses.ProcessManagers.GroupCheckouts;

using static GuestStayAccountEvent;

public static class GroupCheckoutsConfig
{
    public static void ConfigureGroupCheckouts(EventBus eventBus,
        GroupCheckOutFacade groupCheckoutFacade
    )
    {
        eventBus
            .Subscribe<GuestCheckedOut>(groupCheckoutFacade.GuestCheckedOut)
            .Subscribe<GuestCheckOutFailed>(groupCheckoutFacade.GuestCheckOutFailed);
    }
}
