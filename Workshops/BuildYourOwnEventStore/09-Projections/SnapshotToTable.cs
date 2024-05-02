using Dapper;
using Npgsql;

namespace EventStoreBasics;

public class SnapshotToTable<T>(NpgsqlConnection databaseConnection, string upsertSql): ISnapshot
{
    public Type Handles => typeof(T);

    public void Handle(IAggregate aggregate)
    {
        databaseConnection.Execute(upsertSql, aggregate);
    }
}
