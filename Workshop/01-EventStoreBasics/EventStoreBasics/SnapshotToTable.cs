using System;
using Dapper;
using Npgsql;

namespace EventStoreBasics
{
    public class SnapshotToTable<T>: ISnapshot
    {
        private readonly NpgsqlConnection databaseConnection;
        private readonly string upsertSql;

        public SnapshotToTable(NpgsqlConnection databaseConnection, string upsertSql)
        {
            this.databaseConnection = databaseConnection;
            this.upsertSql = upsertSql;
        }

        public Type Handles => typeof(T);

        public void Handle(IAggregate aggregate)
        {
            databaseConnection.Execute(upsertSql, aggregate);
        }
    }
}
