using Database = BusinessProcesses.Core.Database;

namespace BusinessProcesses.ToDoList.GroupCheckouts;

public class ScheduledGroupCheckOutToDoList(Database database, GroupCheckOutToDoList groupCheckoutToDoList)
{
    public async ValueTask HandleAll(CancellationToken ct = default)
    {
        foreach (var groupCheckout in await database.FindAll<GroupCheckOut>(ct))
            await groupCheckoutToDoList.TryComplete(groupCheckout, ct);
    }
}
