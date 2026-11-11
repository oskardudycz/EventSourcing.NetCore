using BusinessProcesses.ToDoList.Core;
using BusinessProcesses.ToDoList.GuestStayAccounts;
using Database = BusinessProcesses.Core.Database;

namespace BusinessProcesses.ToDoList.GroupCheckouts;

using static GroupCheckoutEvent;
using static GuestStayAccountEvent;

public class GroupCheckoutProjection(Database database): Projection<GroupCheckOut>(database)
{
    public override Type[] CanHandle() =>
    [
        typeof(GroupCheckoutInitiated),
        typeof(GuestCheckedOut),
        typeof(GuestCheckOutFailed),
        typeof(GroupCheckoutCompleted),
        typeof(GroupCheckoutFailed)
    ];

    protected override Guid? GetDocumentId(object @event) =>
        @event switch
        {
            GroupCheckoutInitiated initiated => initiated.GroupCheckoutId,
            GuestCheckedOut checkedOut => checkedOut.GroupCheckOutId,
            GuestCheckOutFailed checkOutFailed => checkOutFailed.GroupCheckOutId,
            GroupCheckoutCompleted completed => completed.GroupCheckoutId,
            GroupCheckoutFailed failed => failed.GroupCheckoutId,
            _ => null
        };

    protected override GroupCheckOut? Evolve(GroupCheckOut? current, object @event) =>
        @event switch
        {
            GroupCheckoutInitiated initiated =>
                GroupCheckOut.Initiated(initiated.GroupCheckoutId, initiated.GuestStayIds, initiated.InitiatedAt),
            GuestCheckedOut checkedOut =>
                Existing(current).WithCheckout(checkedOut.GuestStayId, CheckoutStatus.Completed, checkedOut.CheckedOutAt),
            GuestCheckOutFailed checkOutFailed =>
                Existing(current).WithCheckout(checkOutFailed.GuestStayId, CheckoutStatus.Failed, checkOutFailed.FailedAt),
            GroupCheckoutCompleted or GroupCheckoutFailed => null,
            _ => current
        };

    private static GroupCheckOut Existing(GroupCheckOut? current) =>
        current ?? throw new InvalidOperationException("Entity not found");
}
