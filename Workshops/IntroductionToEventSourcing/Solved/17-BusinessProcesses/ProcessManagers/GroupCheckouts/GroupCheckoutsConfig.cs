using BusinessProcesses.Core;
using BusinessProcesses.ProcessManagers.GuestStayAccounts;

namespace BusinessProcesses.ProcessManagers.GroupCheckouts;

using static GuestStayAccountEvent;

public static class GroupCheckoutsConfig
{
    public static void ConfigureGroupCheckouts(EventStore eventStore,
        GroupCheckOutFacade groupCheckoutFacade
    )
    {
        eventStore
            .Subscribe<GuestCheckedOut>(groupCheckoutFacade.GuestCheckedOut)
            .Subscribe<GuestCheckOutFailed>(groupCheckoutFacade.GuestCheckOutFailed);
    }
}
