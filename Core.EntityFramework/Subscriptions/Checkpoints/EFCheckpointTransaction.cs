using Core.EventStoreDB.Subscriptions.Checkpoints;
using Core.EventStoreDB.Subscriptions.Checkpoints.Postgres;
using Microsoft.EntityFrameworkCore;

namespace Core.EntityFramework.Subscriptions.Checkpoints;

public class EFCheckpointTransaction(DbContext dbContext): ICheckpointTransaction
{
    public Task Commit(CancellationToken cancellationToken = default) =>
        dbContext.Database.CommitTransactionAsync(cancellationToken);

    public Task Rollback(CancellationToken cancellationToken = default)
    {
        dbContext.ChangeTracker.Clear();
        return dbContext.Database.RollbackTransactionAsync(cancellationToken);
    }
}
