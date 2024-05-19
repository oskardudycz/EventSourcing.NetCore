using Npgsql;

namespace Core.EventStoreDB.Subscriptions.Checkpoints;

using static ISubscriptionCheckpointRepository;

public class PostgresSubscriptionCheckpointRepository(
    // I'm not using data source here, as I'd like to enable option
    // to update projection in the same transaction in the same transaction as checkpointing
    // to help handling idempotency
    Func<CancellationToken, ValueTask<NpgsqlConnection>> connectionFactory,
    PostgresSubscriptionCheckpointTableCreator checkpointTableCreator
): ISubscriptionCheckpointRepository
{
    public async ValueTask<Checkpoint> Load(string subscriptionId, CancellationToken ct)
    {
        var connection = await connectionFactory(ct).ConfigureAwait(false);
        await checkpointTableCreator.EnsureCheckpointsTableExist(ct).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(SelectCheckpointSql, connection);
        command.Parameters.AddWithValue(subscriptionId);

        await using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);

        return (await reader.ReadAsync(ct).ConfigureAwait(false))
            ? Checkpoint.From(
                await reader.IsDBNullAsync(0, ct).ConfigureAwait(false) ? (ulong)reader.GetInt64(0) : null
            )
            : Checkpoint.None;
    }

    public async ValueTask<StoreResult> Store(
        string subscriptionId,
        ulong position,
        Checkpoint previousCheckpoint, CancellationToken ct)
    {
        var connection = await connectionFactory(ct).ConfigureAwait(false);
        await checkpointTableCreator.EnsureCheckpointsTableExist(ct).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(StoreCheckpointSql, connection);
        command.Parameters.AddWithValue(subscriptionId);
        command.Parameters.AddWithValue(position);

        return await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false) == 1
            ? new StoreResult.Success(Checkpoint.From(position))
            : new StoreResult.Mismatch();
    }

    public async ValueTask<Checkpoint> Reset(string subscriptionId, CancellationToken ct)
    {
        var connection = await connectionFactory(ct).ConfigureAwait(false);
        await checkpointTableCreator.EnsureCheckpointsTableExist(ct).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(ResetCheckpointSql, connection);
        command.Parameters.AddWithValue(subscriptionId);

        await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

        return Checkpoint.None;
    }

    private const string SelectCheckpointSql =
        """
        SELECT "position"
        FROM "subscription_checkpoints"
        WHERE "id" = $1
        """;

    private const string StoreCheckpointSql =
        """
        INSERT INTO "subscription_checkpoints"("id", "position")
        VALUES ($1, $2)
        ON CONFLICT ("id") WHERE "position" = $3
        DO UPDATE SET "position" = $2;
        """;

    private const string ResetCheckpointSql =
        """
        UPDATE "subscription_checkpoints"
        SET "position" = NULL
        WHERE "id" = $1;
        """;
}

public abstract class PostgresSubscriptionCheckpointTableCreator(NpgsqlDataSource dataSource)
{
    private bool wasCreated;
    private readonly SemaphoreSlim tableLock = new(0, 1);

    public async ValueTask EnsureCheckpointsTableExist(CancellationToken ct)
    {
        if (wasCreated)
            return;

        await tableLock.WaitAsync(ct).ConfigureAwait(false);

        if (wasCreated)
            return;

        try
        {
            await using var cmd = dataSource.CreateCommand(CreateCheckpointsTableSql);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            wasCreated = false;
        }
        finally
        {
            tableLock.Release();
        }
    }

    private const string CreateCheckpointsTableSql =
        """
        CREATE TABLE "subscription_checkpoints" (
            "id" VARCHAR(100) PRIMARY KEY,
            "position" BIGINT NULL
        );
        """;
}
