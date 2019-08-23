using System;
using Dapper;
using Npgsql;

namespace EventStoreBasics
{
    public class EventStore: IDisposable, IEventStore
    {
        private readonly NpgsqlConnection databaseConnection;

        public EventStore(NpgsqlConnection databaseConnection)
        {
            this.databaseConnection = databaseConnection;
        }

        public void Init()
        {
            // See more in Greg Young's "Building an Event Storage" article https://cqrs.wordpress.com/documents/building-event-storage/
            CreateStreamsTable();
            CreateEventsTable();
            CreateAppendEventFunction();
        }

        public bool AppendEvent<TStream>(Guid streamId, object @event, long? expectedVersion = null)
        {
            throw new NotImplementedException("Call append_event function with Dapper");
        }

        private void CreateStreamsTable()
        {
            const string CreatStreamsTableSQL =
                @"CREATE TABLE IF NOT EXISTS streams(
                      id             UUID                      NOT NULL    PRIMARY KEY,
                      type           TEXT                      NOT NULL,
                      version        BIGINT                    NOT NULL
                  );";
            databaseConnection.Execute(CreatStreamsTableSQL);
        }

        private void CreateEventsTable()
        {
            const string CreatEventsTableSQL =
                @"CREATE TABLE IF NOT EXISTS events(
                      id             UUID                      NOT NULL    PRIMARY KEY,
                      data           JSONB                     NOT NULL,
                      stream_id      UUID                      NOT NULL,
                      type           TEXT                      NOT NULL,
                      version        BIGINT                    NOT NULL,
                      created        timestamp with time zone  NOT NULL    default (now()),
                      FOREIGN KEY(stream_id) REFERENCES streams(id),
                      CONSTRAINT events_stream_and_version UNIQUE(stream_id, version)
                );";
            databaseConnection.Execute(CreatEventsTableSQL);
        }

        private void CreateAppendEventFunction()
        {
            const string AppendEventFunctionSQL =
                @"CREATE OR REPLACE FUNCTION append_event(id uuid, data text, type text, stream_id uuid, stream_type text, expected_stream_version bigint default null) RETURNS boolean
                LANGUAGE plpgsql
                AS $$
                DECLARE
	                stream_version int;
                BEGIN
                    -- get stream version
                    -- TODO

                    -- if stream doesn't exist - create new one with version 0
                    -- TODO

                    -- check optimistic concurrency - return false if expected stream version is different than stream version
                    -- TODO

                    -- increment stream_version
                    -- TODO

                    -- append event to events table
                    -- TODO

                    -- update stream version in stream table
                    -- TODO

                    -- return true if succeded
                    -- TODO
                END;
                $$;";
            databaseConnection.Execute(AppendEventFunctionSQL);
        }

        public void Dispose()
        {
            databaseConnection.Dispose();
        }
    }
}
