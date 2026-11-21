using BusinessProcesses.Core;
using BusinessProcesses.ToDoList.GuestStayAccounts;

namespace BusinessProcesses.ToDoList.GroupCheckouts;

using static GroupCheckoutCommand;
using static GroupCheckoutEvent;
using static GuestStayAccountCommand;

public class GroupCheckoutFacade(MessageBus messageBus)
{
    public ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default) =>
        messageBus.Publish(
            [
                new GroupCheckoutInitiated(command.GroupCheckoutId, command.ClerkId, command.GuestStayAccountIds, command.Now),
                ..command.GuestStayAccountIds
                    .Select(guestStayAccountId => new CheckOutGuest(guestStayAccountId, command.Now, command.GroupCheckoutId))
            ],
            ct
        );
}

public abstract record GroupCheckoutCommand
{
    public record InitiateGroupCheckout(
        Guid GroupCheckoutId,
        Guid ClerkId,
        Guid[] GuestStayAccountIds,
        DateTimeOffset Now
    ): GroupCheckoutCommand;

    private GroupCheckoutCommand() { }
}
