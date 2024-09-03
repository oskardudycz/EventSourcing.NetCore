using BusinessProcesses.Core;
using BusinessProcesses.Version2_ImmutableEntities.ProcessManagers.GuestStayAccounts;

namespace BusinessProcesses.Version2_ImmutableEntities.ProcessManagers.GroupCheckouts;

using static GuestStayAccountEvent;
using static GroupCheckoutCommand;
using static ProcessManagerResult;

public static class GroupCheckoutsConfig
{
    public static void ConfigureGroupCheckouts(EventBus eventBus,
        GroupCheckoutFacade groupCheckoutFacade
    )
    {
        eventBus
            .Subscribe<GuestCheckedOut>(groupCheckoutFacade.GuestCheckedOut)
            .Subscribe<GuestCheckoutFailed>(groupCheckoutFacade.GuestCheckOutFailed);
    }
}
