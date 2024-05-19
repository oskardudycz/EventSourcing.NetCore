using System.Data;
using Dapper;
using Npgsql;

namespace EventStoreBasics;

public class EventStore(NpgsqlConnection databaseConnection): IDisposable, IEventStore
{
    public void Init()
    {
        // See more in Greg Young's "Building an Event Storage" article https://cqrs.wordpress.com/documents/building-event-storage/
        CreateStreamsTable();
        CreateEventsTable();
        CreateAppendEventFunction();
    }

    public bool AppendEvent<TStream>(Guid streamId, object @event, long? expectedVersion = null)
    {
        // 1. Pass unique event id as Guid (as it's the primary key)
        var eventId = Guid.NewGuid();

        //2. Serialize event data to JSON
        string eventData = null!; // TODO: Add here @event serialization

        //3. Send event type
        string eventType = null!; // TODO: Add here getting event type name

        //4. Send stream type
        string streamType = null!; // TODO: Add here getting stream type

        return databaseConnection.QuerySingle<bool>(
            "SELECT append_event(@Id, @Data::jsonb, @Type, @StreamId, @StreamType, @ExpectedVersion)",
            new
            {
                Id = eventId,
                Data = eventData,
                Type = eventType,
                StreamId = streamId,
                StreamType = streamType,
                ExpectedVersion = expectedVersion ?? 0
            },
            commandType: CommandType.Text
        );
    }

    private void CreateStreamsTable()
    {
        const string creatStreamsTableSql =
            @"CREATE TABLE IF NOT EXISTS streams(
                      id             UUID                      NOT NULL    PRIMARY KEY,
                      type           TEXT                      NOT NULL,
                      version        BIGINT                    NOT NULL
                  );";
        databaseConnection.Execute(creatStreamsTableSql);
    }

    private void CreateEventsTable()
    {
        const string creatEventsTableSql =
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
        databaseConnection.Execute(creatEventsTableSql);
    }

    private void CreateAppendEventFunction()
    {
        const string appendEventFunctionSql =
            @"CREATE OR REPLACE FUNCTION append_event(id uuid, data jsonb, type text, stream_id uuid, stream_type text, expected_stream_version bigint default null) RETURNS boolean
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    stream_version int;
                BEGIN
                    -- 1. get stream version
                    -- TODO

                    -- 2. if stream doesn't exist - create new one with version 0
                    -- TODO

                    -- 3. check optimistic concurrency - return false if expected stream version is different than stream version
                    -- TODO

                    -- 4. increment stream_version
                    -- TODO

                    -- 5. append event to events table
                    -- TODO

                    -- 6. update stream version in stream table
                    -- TODO

                    RETURN TRUE;
                END;
                $$;";
        databaseConnection.Execute(appendEventFunctionSql);
    }

    public void Dispose() =>
        databaseConnection.Dispose();
}
