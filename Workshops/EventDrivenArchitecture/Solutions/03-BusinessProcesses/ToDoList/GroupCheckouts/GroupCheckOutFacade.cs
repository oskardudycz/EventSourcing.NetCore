using BusinessProcesses.Core;
using BusinessProcesses.ToDoList.GuestStayAccounts;

namespace BusinessProcesses.ToDoList.GroupCheckouts;

using static GroupCheckoutCommand;
using static GroupCheckoutEvent;
using static GuestStayAccountCommand;

public class GroupCheckOutFacade(MessageBus messageBus)
{
    public ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default) =>
        messageBus.Publish(
            [
                new GroupCheckoutInitiated(command.GroupCheckoutId, command.ClerkId, command.GuestStayIds, command.Now),
                ..command.GuestStayIds
                    .Select(guestStayId => new CheckOutGuest(guestStayId, command.Now, command.GroupCheckoutId))
            ],
            ct
        );
}

public abstract record GroupCheckoutCommand
{
    public record InitiateGroupCheckout(
        Guid GroupCheckoutId,
        Guid ClerkId,
        Guid[] GuestStayIds,
        DateTimeOffset Now
    ): GroupCheckoutCommand;

    private GroupCheckoutCommand() { }
}
