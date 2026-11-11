using BusinessProcesses.Core;
using BusinessProcesses.ProcessManagers.GuestStayAccounts;

namespace BusinessProcesses.ProcessManagers.GroupCheckouts;

using static GuestStayAccountEvent;

public static class GroupCheckoutsConfig
{
    public static void ConfigureGroupCheckouts(MessageBus messageBus,
        GroupCheckOutFacade groupCheckoutFacade
    )
    {
        messageBus
            .Subscribe<GuestCheckedOut>(groupCheckoutFacade.GuestCheckedOut)
            .Subscribe<GuestCheckOutFailed>(groupCheckoutFacade.GuestCheckOutFailed);
    }
}
