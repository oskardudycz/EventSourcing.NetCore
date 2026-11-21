using Database = BusinessProcesses.Core.Database;

namespace BusinessProcesses.ToDoList.GroupCheckouts;

public class ScheduledGroupCheckoutToDoList(Database database, GroupCheckoutToDoList groupCheckoutToDoList)
{
    public async ValueTask HandleAll(CancellationToken ct = default)
    {
        foreach (var groupCheckout in await database.FindAll<GroupCheckout>(ct))
            await groupCheckoutToDoList.TryComplete(groupCheckout, ct);
    }
}
