using BusinessProcesses.ToDoList.GuestStayAccounts;
using BusinessProcesses.Core;

namespace BusinessProcesses.ToDoList.GroupCheckouts;

using static GuestStayAccountEvent;

public static class GroupCheckoutsConfig
{
    public static void ConfigureGroupCheckouts(
        EventBus eventBus,
        GroupCheckOutToDoList groupCheckoutToDoList
    )
    {
        eventBus
            .Subscribe<GroupCheckoutEvent.GroupCheckoutInitiated>(groupCheckoutToDoList.GroupCheckoutInitiated)
            .Subscribe<GuestCheckedOut>(groupCheckoutToDoList.GuestCheckedOut)
            .Subscribe<GuestCheckOutFailed>(groupCheckoutToDoList.GuestCheckOutFailed);
    }
}
