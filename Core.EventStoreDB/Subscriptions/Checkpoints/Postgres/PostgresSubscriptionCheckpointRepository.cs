using System.Data;
using Npgsql;

namespace Core.EventStoreDB.Subscriptions.Checkpoints.Postgres;

using static ISubscriptionCheckpointRepository;

public class PostgresConnectionProvider(Func<CancellationToken, ValueTask<NpgsqlConnection>> connectionFactory)
{
    public ValueTask<NpgsqlConnection> Get(CancellationToken ct) => connectionFactory(ct);

    public void Set(NpgsqlConnection connection) =>
        connectionFactory = _ => ValueTask.FromResult(connection);

    public void Set(NpgsqlTransaction transaction) =>
        connectionFactory = _ => ValueTask.FromResult(transaction.Connection!);

    public void Set(NpgsqlDataSource dataSource) =>
        connectionFactory = async ct =>
        {
            var connection = dataSource.CreateConnection();
            await connection.OpenAsync(ct).ConfigureAwait(false);
            return connection;
        };

    public static PostgresConnectionProvider From(NpgsqlDataSource npgsqlDataSource) =>
        new(async ct =>
        {
            var connection = npgsqlDataSource.CreateConnection();
            await connection.OpenAsync(ct).ConfigureAwait(false);
            return connection;
        });

    public static PostgresConnectionProvider From(NpgsqlTransaction transaction) =>
        new(async ct =>
        {
            if (transaction.Connection == null)
                throw new InvalidOperationException("Transaction connection is not opened!");

            if (transaction.Connection.State == ConnectionState.Closed)
                await transaction.Connection.OpenAsync(ct).ConfigureAwait(false);

            return transaction.Connection;
        });
}

public class PostgresSubscriptionCheckpointRepository(
    // I'm not using data source here, as I'd like to enable option
    // to update projection in the same transaction in the same transaction as checkpointing
    // to help handling idempotency
    PostgresConnectionProvider connectionProvider
): ISubscriptionCheckpointRepository
{
    public async ValueTask<Checkpoint> Load(string subscriptionId, CancellationToken ct)
    {
        var connection = await connectionProvider.Get(ct).ConfigureAwait(false);

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
        Checkpoint previousCheckpoint,
        CancellationToken ct
    )
    {
        var connection = await connectionProvider.Get(ct).ConfigureAwait(false);

        await using var command = new NpgsqlCommand("SELECT store_subscription_checkpoint($1, $2, $3)", connection);
        command.Parameters.AddWithValue(subscriptionId);
        command.Parameters.AddWithValue((long)position);
        command.Parameters.AddWithValue(previousCheckpoint != Checkpoint.None
            ? (long)previousCheckpoint.Position
            : DBNull.Value);

        var result = await command.ExecuteScalarAsync(ct).ConfigureAwait(false);

        return result switch
        {
            1 => new StoreResult.Success(Checkpoint.From(position)),
            0 => new StoreResult.Ignored(),
            2 => new StoreResult.Mismatch(),
            _ => new StoreResult.Mismatch(),
        };
    }

    public async ValueTask<Checkpoint> Reset(string subscriptionId, CancellationToken ct)
    {
        var connection = await connectionProvider.Get(ct).ConfigureAwait(false);

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

public interface ICheckpointTransaction
{
    Task Commit(CancellationToken cancellationToken = default);

    Task Rollback(CancellationToken cancellationToken = default);
}

public class TransactionalPostgresSubscriptionCheckpointRepository(
    PostgresSubscriptionCheckpointRepository inner,
    ICheckpointTransaction checkpointTransaction
): ISubscriptionCheckpointRepository
{
    public ValueTask<Checkpoint> Load(string subscriptionId, CancellationToken ct) =>
        inner.Load(subscriptionId, ct);

    public async ValueTask<StoreResult> Store(
        string subscriptionId,
        ulong position,
        Checkpoint previousCheckpoint,
        CancellationToken ct
    )
    {
        var result = await inner.Store(subscriptionId, position, previousCheckpoint, ct).ConfigureAwait(false);

        if (result is not StoreResult.Success)
        {
            await checkpointTransaction.Rollback(ct).ConfigureAwait(false);
            return result;
        }

        await checkpointTransaction.Commit(ct).ConfigureAwait(false);

        return result;
    }

    public ValueTask<Checkpoint> Reset(string subscriptionId, CancellationToken ct) =>
        inner.Reset(subscriptionId, ct);
}
