using System;
using System.Data;
using System.Linq;
using Dapper;
using Newtonsoft.Json;
using Npgsql;

namespace EventStoreBasics
{
    public class EventStore : IDisposable
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

        public bool AppendEvent<TStream, TEvent>(Guid streamId, TEvent @event, long? expectedVersion = null)
        {
            return databaseConnection.QuerySingle<bool>(
                "SELECT append_event(@Id, @Data, @Type, @StreamId, @StreamType, @ExpectedVersion)",
                new
                {
                    Id = Guid.NewGuid(),
                    Data = JsonConvert.SerializeObject(@event),
                    Type = typeof(TEvent).FullName,
                    StreamId = streamId,
                    StreamType = typeof(TStream).FullName,
                    ExpectedVersion = expectedVersion
                },
                commandType: CommandType.Text
            );
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
                      stream_id       UUID                      NOT NULL,
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
                    SELECT
                        version INTO stream_version
                    FROM streams as s
                    WHERE
                        s.id = stream_id FOR UPDATE;

                    -- if stream doesn't exist - create new one with version 0
                    IF stream_version IS NULL THEN
                        stream_version := 0;

                        INSERT INTO streams
                            (id, type, version)
                        VALUES
                            (stream_id, stream_type, stream_version);
                    END IF;

                    -- check optimistic concurrency
                    IF expected_stream_version IS NOT NULL AND stream_version != expected_stream_version THEN
                        RETURN FALSE;
                    END IF;

                    -- increment event_version
                    stream_version := stream_version + 1;

                    -- append event
                    INSERT INTO events
			            (id, data, stream_id, type, version)
		            VALUES
			            (id, data::jsonb, stream_id, type, stream_version);

                    -- update stream version
                    UPDATE streams as s
                        SET version = stream_version
                    WHERE
                        s.id = stream_id;

                    RETURN TRUE;
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
