using Npgsql;

namespace Core.EventStoreDB.Subscriptions.Checkpoints.Postgres;

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
