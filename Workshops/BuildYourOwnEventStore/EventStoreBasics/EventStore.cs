using System.Collections;
using System.Data;
using Dapper;
using EventStoreBasics.Tools;
using Newtonsoft.Json;
using Npgsql;

namespace EventStoreBasics;

public class EventStore(NpgsqlConnection databaseConnection): IAsyncDisposable, IEventStore
{
    private readonly IList<ISnapshot> snapshots = new List<ISnapshot>();
    private readonly IList<IProjection> projections = new List<IProjection>();

    private const string Apply = "Apply";

    public async Task Init(CancellationToken ct = default)
    {
        // See more in Greg Young's "Building an Event Storage" article https://cqrs.wordpress.com/documents/building-event-storage/
        await CreateStreamsTable(ct);
        await CreateEventsTable(ct);
        await CreateAppendEventFunction(ct);
    }

    public void AddSnapshot(ISnapshot snapshot) =>
        snapshots.Add(snapshot);

    public void AddProjection(IProjection projection) =>
        projections.Add(projection);

    public async Task<bool> Store<TStream>(TStream aggregate, CancellationToken ct = default) where TStream : IAggregate
    {
        var events = aggregate.DequeueUncommittedEvents();
        var initialVersion = aggregate.Version - events.Count();

        foreach (var @event in events)
        {
            await AppendEvent<TStream>(aggregate.Id, @event, initialVersion++, ct);

            foreach (var projection in projections.Where(
                         projection => projection.Handles.Contains(@event.GetType())))
            {
                projection.Handle(@event);
            }
        }

        snapshots
            .FirstOrDefault(snapshot => snapshot.Handles == typeof(TStream))?
            .Handle(aggregate);

        return true;
    }

    public Task<bool> AppendEvent<TStream>(Guid streamId, object @event, long? expectedVersion = null,
        CancellationToken ct = default)
        where TStream : notnull =>
        databaseConnection.QuerySingleAsync<bool>(
            "SELECT append_event(@Id, @Data::jsonb, @Type, @StreamId, @StreamType, @ExpectedVersion)",
            new
            {
                Id = Guid.CreateVersion7(),
                Data = JsonConvert.SerializeObject(@event),
                Type = @event.GetType().AssemblyQualifiedName,
                StreamId = streamId,
                StreamType = typeof(TStream).AssemblyQualifiedName,
                ExpectedVersion = expectedVersion
            },
            commandType: CommandType.Text
        );

    public async Task<T?> AggregateStream<T>(Guid streamId, long? atStreamVersion = null, DateTime? atTimestamp = null,
        CancellationToken ct = default)
        where T : notnull
    {
        var aggregate = (T)Activator.CreateInstance(typeof(T), true)!;

        var events = await GetEvents(streamId, atStreamVersion, atTimestamp, ct);
        var version = 0;

        foreach (var @event in events)
        {
            aggregate.InvokeIfExists(Apply, @event);
            aggregate.SetIfExists(nameof(IAggregate.Version), ++version);
        }

        return aggregate;
    }

    public async Task<StreamState?> GetStreamState(Guid streamId, CancellationToken ct = default)
    {
        const string getStreamSql =
            @"SELECT id, type, version
                  FROM streams
                  WHERE id = @streamId";

        var result = await databaseConnection
            .QueryAsync<dynamic>(getStreamSql, new { streamId });

        return result
            .Select(streamData =>
                new StreamState(
                    streamData.id,
                    Type.GetType(streamData.type, true)!,
                    streamData.version
                ))
            .SingleOrDefault();
    }

    public async Task<IEnumerable> GetEvents(Guid streamId, long? atStreamVersion = null, DateTime? atTimestamp = null,
        CancellationToken ct = default)
    {
        var atStreamCondition = atStreamVersion != null ? "AND version <= @atStreamVersion" : string.Empty;
        var atTimestampCondition = atTimestamp != null ? "AND created <= @atTimestamp" : string.Empty;

        var getStreamSql =
            @$"SELECT id, data, stream_id, type, version, created
                  FROM events
                  WHERE stream_id = @streamId
                  {atStreamCondition}
                  {atTimestampCondition}
                  ORDER BY version";

        var result = await databaseConnection
            .QueryAsync<dynamic>(getStreamSql, new { streamId, atStreamVersion, atTimestamp });

        return result
            .Select(@event =>
                JsonConvert.DeserializeObject(
                    @event.data,
                    Type.GetType(@event.type, true)!
                ))
            .ToList();
    }

    private Task CreateStreamsTable(CancellationToken ct)
    {
        const string createStreamsTableSql =
            @"CREATE TABLE IF NOT EXISTS streams(
                      id             UUID                      NOT NULL    PRIMARY KEY,
                      type           TEXT                      NOT NULL,
                      version        BIGINT                    NOT NULL
                  );";
        return databaseConnection.ExecuteAsync(createStreamsTableSql);
    }

    private Task CreateEventsTable(CancellationToken ct)
    {
        const string createEventsTableSql =
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
        return databaseConnection.ExecuteAsync(createEventsTableSql);
    }

    private Task CreateAppendEventFunction(CancellationToken ct)
    {
        const string appendEventFunctionSql =
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
                        (id, data::jsonb, stream_id, type, stream_version);

                    -- update stream version
                    UPDATE streams as s
                        SET version = stream_version
                    WHERE
                        s.id = stream_id;

                    RETURN TRUE;
                END;
                $$;";
        return databaseConnection.ExecuteAsync(appendEventFunctionSql);
    }

    public ValueTask DisposeAsync() =>
        databaseConnection.DisposeAsync();
}
