using BusinessProcesses.Core;

namespace BusinessProcesses.ToDoList.GroupCheckouts;

using static GroupCheckoutEvent;

public class GroupCheckOutToDoList(GroupCheckoutProjection projection, MessageBus messageBus)
{
    public Type[] CanHandle() => projection.CanHandle();

    public async ValueTask Handle(object @event, CancellationToken ct = default)
    {
        var groupCheckout = await projection.Handle(@event, ct);

        if (groupCheckout != null)
            await TryComplete(groupCheckout, ct);
    }

    // This could be also checked by Cron
    public ValueTask TryComplete(GroupCheckOut groupCheckout, CancellationToken ct = default)
    {
        if (groupCheckout.AreAnyOngoingCheckouts())
            return ValueTask.CompletedTask;

        return messageBus.Publish(
            [
                !groupCheckout.AreAnyFailedCheckouts()
                    ? new GroupCheckoutCompleted(
                        groupCheckout.Id,
                        groupCheckout.CheckoutsWith(CheckoutStatus.Completed),
                        groupCheckout.UpdatedAt
                    )
                    : new GroupCheckoutFailed(
                        groupCheckout.Id,
                        groupCheckout.CheckoutsWith(CheckoutStatus.Completed),
                        groupCheckout.CheckoutsWith(CheckoutStatus.Failed),
                        groupCheckout.UpdatedAt
                    )
            ],
            ct
        );
    }
}
