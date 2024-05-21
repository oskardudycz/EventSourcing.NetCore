using Core.Events;
using Core.EventStoreDB.Subscriptions;
using Core.EventStoreDB.Subscriptions.Batch;
using Core.EventStoreDB.Subscriptions.Checkpoints;
using Core.EventStoreDB.Subscriptions.Checkpoints.Postgres;
using EventStore.Client;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Core.EntityFramework.Subscriptions.Checkpoints;

using static ISubscriptionCheckpointRepository;

public class TransactionalDbContextEventsBatchCheckpointer<TDbContext>(
    TDbContext dbContext,
    NpgsqlConnection connection,
    NpgsqlTransaction transaction,
    EventsBatchProcessor batchProcessor
): IEventsBatchCheckpointer
    where TDbContext : DbContext
{
    public async Task<StoreResult> Process(
        ResolvedEvent[] events,
        Checkpoint lastCheckpoint,
        BatchProcessingOptions options,
        CancellationToken ct
        )
    {
        await dbContext.Database.UseTransactionAsync(transaction, ct);
        var inner = new EventsBatchCheckpointer(
            new PostgresSubscriptionCheckpointRepository(connection, transaction),
            batchProcessor
        );

        var result = await inner.Process(events, lastCheckpoint, options, ct)
            .ConfigureAwait(false);

        await dbContext.SaveChangesAsync(ct);

        if (result is not StoreResult.Success)
        {
            dbContext.ChangeTracker.Clear();
            await transaction.RollbackAsync(ct);
            return result;
        }

        await transaction.CommitAsync(ct).ConfigureAwait(false);

        return result;
    }
}
