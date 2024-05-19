using Npgsql;

namespace Core.EventStoreDB.Subscriptions.Checkpoints;

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
}

public class PostgresSubscriptionCheckpointRepository(
    // I'm not using data source here, as I'd like to enable option
    // to update projection in the same transaction in the same transaction as checkpointing
    // to help handling idempotency
    PostgresConnectionProvider connectionProvider,
    PostgresSubscriptionCheckpointSetup checkpointSetup
): ISubscriptionCheckpointRepository
{
    public async ValueTask<Checkpoint> Load(string subscriptionId, CancellationToken ct)
    {
        var connection = await connectionProvider.Get(ct).ConfigureAwait(false);
        await checkpointSetup.EnsureCheckpointsTableExist(ct).ConfigureAwait(false);

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
        await checkpointSetup.EnsureCheckpointsTableExist(ct).ConfigureAwait(false);

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
        await checkpointSetup.EnsureCheckpointsTableExist(ct).ConfigureAwait(false);

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

public class PostgresSubscriptionCheckpointSetup(NpgsqlDataSource dataSource)
{
    private bool wasCreated;
    private readonly SemaphoreSlim tableLock = new(1, 1);

    public async ValueTask EnsureCheckpointsTableExist(CancellationToken ct)
    {
        if (wasCreated)
            return;

        await tableLock.WaitAsync(ct).ConfigureAwait(false);

        if (wasCreated)
            return;

        try
        {
            await using var cmd = dataSource.CreateCommand(SetupSql);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            wasCreated = false;
        }
        finally
        {
            tableLock.Release();
        }
    }

    private const string SetupSql = $"{CreateCheckpointsTableSql}\n{CreateStoreCheckpointsProcedureSql}";

    private const string CreateCheckpointsTableSql =
        """
        CREATE TABLE IF NOT EXISTS "subscription_checkpoints" (
            "id" VARCHAR(100) PRIMARY KEY,
            "position" BIGINT NULL,
            "revision" BIGINT
        );
        """;

    private const string CreateStoreCheckpointsProcedureSql =
        """
        CREATE OR REPLACE FUNCTION store_subscription_checkpoint(
            p_id VARCHAR(100),
            p_position BIGINT,
            check_position BIGINT DEFAULT NULL
        ) RETURNS INT AS $$
        DECLARE
            current_position BIGINT;
        BEGIN
            -- Handle the case when check_position is provided
            IF check_position IS NOT NULL THEN
                -- Try to update if the position matches check_position
                UPDATE "subscription_checkpoints"
                SET "position" = p_position
                WHERE "id" = p_id AND "position" = check_position;

                IF FOUND THEN
                    RETURN 1;  -- Successfully updated
                END IF;

                -- Retrieve the current position
                SELECT "position" INTO current_position
                FROM "subscription_checkpoints"
                WHERE "id" = p_id;

                -- Return appropriate codes based on current position
                IF current_position = p_position THEN
                    RETURN 0;  -- Idempotent check: position already set
                ELSIF current_position > check_position THEN
                    RETURN 2;  -- Failure: current position is greater
                ELSE
                    RETURN 2;  -- Default failure case for mismatched positions
                END IF;
            END IF;

            -- Handle the case when check_position is NULL: Insert if not exists
            BEGIN
                INSERT INTO "subscription_checkpoints"("id", "position")
                VALUES (p_id, p_position);
                RETURN 1;  -- Successfully inserted
            EXCEPTION WHEN unique_violation THEN
                -- If insertion failed, it means the row already exists
                SELECT "position" INTO current_position
                FROM "subscription_checkpoints"
                WHERE "id" = p_id;

                IF current_position = p_position THEN
                    RETURN 0;  -- Idempotent check: position already set
                ELSE
                    RETURN 2;  -- Insertion failed, row already exists with different position
                END IF;
            END;
        END;
        $$ LANGUAGE plpgsql;
        """;
}
