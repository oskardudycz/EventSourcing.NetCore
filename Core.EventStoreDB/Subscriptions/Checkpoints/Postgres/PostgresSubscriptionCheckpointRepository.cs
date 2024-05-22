using System.Data;
using Npgsql;
using Polly;

namespace Core.EventStoreDB.Subscriptions.Checkpoints.Postgres;

using static ISubscriptionCheckpointRepository;

public class PostgresSubscriptionCheckpointRepository(
    // I'm not using data source here, as I'd like to enable option
    // to update projection in the same transaction in the same transaction as checkpointing
    // to help handling idempotency
    NpgsqlConnection connection,
    NpgsqlTransaction? transaction = null
): ISubscriptionCheckpointRepository
{
    public async ValueTask<Checkpoint> Load(string subscriptionId, CancellationToken ct)
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(ct).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = SelectCheckpointSql;
        command.Parameters.AddWithValue(subscriptionId);

        await using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);

        if(!await reader.ReadAsync(ct).ConfigureAwait(false))
            return Checkpoint.None;

        var value = reader.GetValue(0);

        var checkpoint = Checkpoint.From(Convert.ToUInt64(value));

        return checkpoint;
    }

    public async ValueTask<StoreResult> Store(
        string subscriptionId,
        ulong position,
        Checkpoint previousCheckpoint,
        CancellationToken ct
    )
    {
        if (connection.State == ConnectionState.Closed)
            await connection.OpenAsync(ct).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT store_subscription_checkpoint($1, $2, $3)";
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
