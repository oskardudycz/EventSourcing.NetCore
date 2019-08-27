using System;
using System.Collections;
using System.Data;
using System.Linq;
using Dapper;
using EventStoreBasics.Tools;
using Newtonsoft.Json;
using Npgsql;

namespace EventStoreBasics
{
    public class EventStore: IDisposable, IEventStore
    {
        private readonly NpgsqlConnection databaseConnection;

        private const string Apply = "Apply";
        private const string Version = "Version";

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
            return databaseConnection.QuerySingle<bool>(
                "SELECT append_event(@Id, @Data::jsonb, @Type, @StreamId, @StreamType, @ExpectedVersion)",
                new
                {
                    Id = Guid.NewGuid(),
                    Data = JsonConvert.SerializeObject(@event),
                    Type = @event.GetType().AssemblyQualifiedName,
                    StreamId = streamId,
                    StreamType = typeof(TStream).AssemblyQualifiedName,
                    ExpectedVersion = expectedVersion
                },
                commandType: CommandType.Text
            );
        }

        public T AggregateStream<T>(Guid streamId, long? atStreamVersion = null, DateTime? atTimestamp = null)
        {
            // TODO aggregate only events that are smaller or bigger than timestamp or version
            var aggregate = (T)Activator.CreateInstance(typeof(T), true);

            var events = GetEvents(streamId);
            var version = 0;

            foreach (var @event in events)
            {
                aggregate.InvokeIfExists(Apply, @event);
                aggregate.SetIfExists(Version, ++version);
            }

            return aggregate;
        }

        public StreamState GetStreamState(Guid streamId)
        {
            const string GetStreamSQL =
                @"SELECT id, type, version
                  FROM streams
                  WHERE id = @streamId";

            return databaseConnection
                .Query<dynamic>(GetStreamSQL, new { streamId })
                .Select(streamData =>
                    new StreamState(
                        streamData.id,
                        Type.GetType(streamData.type),
                        streamData.version
                    ))
                .SingleOrDefault();
        }

        public IEnumerable GetEvents(Guid streamId, long? atStreamVersion = null, DateTime? atTimestamp = null)
        {
            // TODO add getting only events that are smaller or bigger than timestamp or version
            const string GetStreamSQL =
                @"SELECT id, data, stream_id, type, version, created
                  FROM events
                  WHERE stream_id = @streamId
                  ORDER BY version";

            return databaseConnection
                .Query<dynamic>(GetStreamSQL, new { streamId, atStreamVersion, atTimestamp })
                .Select(@event =>
                    JsonConvert.DeserializeObject(
                        @event.data,
                        Type.GetType(@event.type)
                    ))
                .ToList();
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
                @"CREATE OR REPLACE FUNCTION append_event(id uuid, data jsonb, type text, stream_id uuid, stream_type text, expected_stream_version bigint default null) RETURNS boolean
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
                        (id, data, stream_id, type, stream_version);

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
