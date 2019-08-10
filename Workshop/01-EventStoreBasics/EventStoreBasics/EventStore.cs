using System;
using System.Data;
using Dapper;
using Npgsql;

namespace EventStoreBasics
{
    public class EventStore : IDisposable
    {
        private readonly NpgsqlConnection databaseConnection;

        const string CreatStreamsTable =
            @"CREATE TABLE IF NOT EXISTS streams(
                  id             UUID    NOT NULL    PRIMARY KEY,
                  type           TEXT    NOT NULL,
                  version        BIGINT  NOT NULL
              );";

        public EventStore(NpgsqlConnection databaseConnection)
        {
            this.databaseConnection = databaseConnection;
        }

        public void Init()
        {
            if (databaseConnection.State != ConnectionState.Open)
            {
                databaseConnection.Open();
            }
            using (var transaction = databaseConnection.BeginTransaction())
            {
                CreateStreamsTable(transaction);

                transaction.Commit();
            }
        }

        private void CreateStreamsTable(IDbTransaction transaction)
        {
            databaseConnection.Execute(CreatStreamsTable);
        }

        public void Dispose()
        {
            databaseConnection.Dispose();
        }
    }
}
