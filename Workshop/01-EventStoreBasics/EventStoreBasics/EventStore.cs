using System;
using Dapper;
using Npgsql;

namespace EventStoreBasics
{
    public class EventStore : IDisposable
    {
        private readonly NpgsqlConnection databaseConnection;

        const string CreatStreamsTableSQL =
            @"CREATE TABLE IF NOT EXISTS streams(
                  id             UUID                      NOT NULL    PRIMARY KEY,
                  type           TEXT                      NOT NULL,
                  version        BIGINT                    NOT NULL
              );";


        const string CreatEventsTableSQL =
            @"CREATE TABLE IF NOT EXISTS events(
                  id             UUID                      NOT NULL    PRIMARY KEY,
                  data           JSONB                     NOT NULL,
                  streamid       UUID                      NOT NULL,
                  type           TEXT                      NOT NULL,
                  version        BIGINT                    NOT NULL,
                  created        timestamp with time zone  NOT NULL    default (now()),
                  FOREIGN KEY(streamid) REFERENCES streams(id),
                  CONSTRAINT events_stream_and_version UNIQUE(streamid, version)
              );";

        public EventStore(NpgsqlConnection databaseConnection)
        {
            this.databaseConnection = databaseConnection;
        }

        public void Init()
        {
            CreateStreamsTable();
            CreateEventsTable();
        }

        private void CreateStreamsTable()
        {
            databaseConnection.Execute(CreatStreamsTableSQL);
        }

        private void CreateEventsTable()
        {
            databaseConnection.Execute(CreatEventsTableSQL);
        }

        public void Dispose()
        {
            databaseConnection.Dispose();
        }
    }
}
