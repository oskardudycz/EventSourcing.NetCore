using BusinessProcesses.Core;

namespace BusinessProcesses.Version1_Aggregates.GroupCheckouts;

public static class GroupCheckoutsConfig
{
    public static void ConfigureGroupCheckouts(
        MessageBus messageBus,
        GroupCheckoutFacade groupCheckoutFacade
    )
    {
        messageBus.Subscribe<GroupCheckoutEvent.GroupCheckoutInitiated>((@event, ct) =>
            throw new NotImplementedException("Handle GroupCheckout Initiated event")
        );
        // Add other message bus subscriptions for commands and events when needed
        throw new NotImplementedException("Configure group checkouts handlers here");
    }
}
