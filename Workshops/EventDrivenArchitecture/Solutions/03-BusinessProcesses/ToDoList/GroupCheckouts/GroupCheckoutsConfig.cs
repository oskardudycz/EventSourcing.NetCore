using BusinessProcesses.Core;

namespace BusinessProcesses.ToDoList.GroupCheckouts;

public static class GroupCheckoutsConfig
{
    public static void ConfigureGroupCheckouts(
        MessageBus messageBus,
        GroupCheckoutToDoList groupCheckoutToDoList
    )
    {
        foreach (var eventType in groupCheckoutToDoList.CanHandle())
            messageBus.Subscribe(eventType, groupCheckoutToDoList.Handle);
    }
}
