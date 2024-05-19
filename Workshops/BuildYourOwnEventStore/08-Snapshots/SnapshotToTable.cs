using Npgsql;

namespace EventStoreBasics;

public class SnapshotToTable<T>(NpgsqlConnection databaseConnection, string upsertSql): ISnapshot
{
    private readonly NpgsqlConnection databaseConnection = databaseConnection;
    private readonly string upsertSql = upsertSql;

    public Type Handles => typeof(T);

    public void Handle(IAggregate aggregate) =>
        throw new NotImplementedException("Call upsert statement");
}
