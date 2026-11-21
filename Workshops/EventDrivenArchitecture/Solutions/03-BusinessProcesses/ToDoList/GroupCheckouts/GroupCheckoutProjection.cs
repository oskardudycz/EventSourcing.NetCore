using BusinessProcesses.ToDoList.Core;
using BusinessProcesses.ToDoList.GuestStayAccounts;
using Database = BusinessProcesses.Core.Database;

namespace BusinessProcesses.ToDoList.GroupCheckouts;

using static GroupCheckoutEvent;
using static GuestStayAccountEvent;

public class GroupCheckoutProjection(Database database): Projection<GroupCheckout>(database)
{
    public override Type[] CanHandle() =>
    [
        typeof(GroupCheckoutInitiated),
        typeof(GuestCheckedOut),
        typeof(GuestCheckoutFailed),
        typeof(GroupCheckoutCompleted),
        typeof(GroupCheckoutFailed)
    ];

    protected override Guid? GetDocumentId(object @event) =>
        @event switch
        {
            GroupCheckoutInitiated initiated => initiated.GroupCheckoutId,
            GuestCheckedOut checkedOut => checkedOut.GroupCheckoutId,
            GuestCheckoutFailed checkOutFailed => checkOutFailed.GroupCheckoutId,
            GroupCheckoutCompleted completed => completed.GroupCheckoutId,
            GroupCheckoutFailed failed => failed.GroupCheckoutId,
            _ => null
        };

    protected override GroupCheckout? Evolve(GroupCheckout? current, object @event) =>
        @event switch
        {
            GroupCheckoutInitiated initiated =>
                GroupCheckout.Initiated(initiated.GroupCheckoutId, initiated.GuestStayAccountIds, initiated.InitiatedAt),
            GuestCheckedOut checkedOut =>
                current?.WithCheckout(checkedOut.GuestStayAccountId, CheckoutStatus.Completed, checkedOut.CompletedAt),
            GuestCheckoutFailed checkOutFailed =>
                current?.WithCheckout(checkOutFailed.GuestStayAccountId, CheckoutStatus.Failed, checkOutFailed.FailedAt),
            GroupCheckoutCompleted or GroupCheckoutFailed => null,
            _ => current
        };
}
