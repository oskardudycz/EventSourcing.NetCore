using Shouldly;
using Tools.Tools;
using Xunit;

namespace EventStoreBasics.Tests;

/// <summary>
/// Exercise 02- Create Events Table
/// </summary>
/// <remarks>
/// <para>
/// Events table is the main table for Event Sourcing storage. It contains the information about events
/// that occurred in the system. Each event is stored in separate row.
/// </para>
/// <para>
/// They're stored as key/value pair (id + event data) plus additional data like stream id, version, type, creation timestamp.
/// </para>
/// <para>
/// So the full list of the Events Table columns is:
/// </para>
/// <list type="bullet">
/// <item>
///     <term><c>Id</c></term>
///     <description>unique event identifier</description>
/// </item>
/// <item>
///     <term><c>Data</c></term>
///     <description>Event data serialized as JSON</description>
/// </item>
/// <item>
///     <term><c>StreamId</c></term>
///     <description>id of the stream that event occured</description>
/// </item>
/// <item>
///     <term><c>Type</c></term>
///     <description>information about the event type. It' mostly used to make debugging easier or some optimizations.</description>
/// </item>
/// <item>
///     <term><c>Version</c></term>
///     <description>version of the stream at which event occured used for keeping sequence of the event and for optimistic concurrency check</description>
/// </item>
/// <item>
///     <term><c>Created</c></term>
///     <description>Timestamp at which event was created. Used to get the state of the stream at exact time.</description>
/// </item>
/// </list>
/// <para>
/// Class provides set of tests verifying if <see cref="EventStore.Init()"/> method initializes <c>Events</c> table properly.
/// </para>
/// </remarks>
public class Exercise02CreateEventsTable : IAsyncLifetime
{
    private readonly PostgresSchemaProvider schemaProvider;
    private readonly EventStore eventStore;

    private const string EventsTableName = "events";

    private const string IdColumnName = "id";
    private const string DataColumnName = "data";
    private const string StreamIdColumnName = "stream_id";
    private const string TypeColumnName = "type";
    private const string VersionColumnName = "version";
    private const string CreatedColumnName = "created";

    /// <summary>
    /// Inits Event Store
    /// </summary>
    public Exercise02CreateEventsTable()
    {
        var databaseConnection1 = PostgresDbConnectionProvider.GetFreshDbConnection();
        schemaProvider = new PostgresSchemaProvider(databaseConnection1);

        // Create Event Store
        eventStore = new EventStore(databaseConnection1);
    }

    /// <summary>
    /// Verifies if Stream table was created
    /// </summary>
    [Fact]
    public void EventsTable_ShouldBeCreated()
    {
        var streamsTable = schemaProvider.GetTable(EventsTableName);

        streamsTable.ShouldNotBeNull();
        streamsTable!.Name.ShouldBe(EventsTableName);
    }

    /// <summary>
    /// Verifies if Stream table has Id column of type Guid
    /// </summary>
    [Fact]
    public void EventsTable_ShouldHave_IdColumn()
    {
        var idColumn = schemaProvider
            .GetTable(EventsTableName)?
            .GetColumn(IdColumnName);

        idColumn.ShouldNotBeNull();
        idColumn!.Name.ShouldBe(IdColumnName);
        idColumn.Type.ShouldBe(Column.GuidType);
    }

    /// <summary>
    /// Verifies if Stream table has Id column of type Guid
    /// </summary>
    [Fact]
    public void EventsTable_ShouldHave_DataColumn_WithJsonType()
    {
        var dataColumn = schemaProvider
            .GetTable(EventsTableName)?
            .GetColumn(DataColumnName);

        dataColumn.ShouldNotBeNull();
        dataColumn!.Name.ShouldBe(DataColumnName);
        dataColumn.Type.ShouldBe(Column.JsonType);
    }

    /// <summary>
    /// Verifies if Stream table has StreamId column of type Guid
    /// </summary>
    [Fact]
    public void EventsTable_ShouldHave_StreamIdColumn_WithGuidType()
    {
        var dataColumn = schemaProvider
            .GetTable(EventsTableName)?
            .GetColumn(StreamIdColumnName);

        dataColumn.ShouldNotBeNull();
        dataColumn!.Name.ShouldBe(StreamIdColumnName);
        dataColumn.Type.ShouldBe(Column.GuidType);
    }

    /// <summary>
    /// Verifies if Stream table has Type column of type String
    /// </summary>
    [Fact]
    public void EventsTable_ShouldHave_TypeColumn_WithStringType()
    {
        var typeColumn = schemaProvider
            .GetTable(EventsTableName)?
            .GetColumn(TypeColumnName);

        typeColumn.ShouldNotBeNull();
        typeColumn!.Name.ShouldBe(TypeColumnName);
        typeColumn.Type.ShouldBe(Column.StringType);
    }

    /// <summary>
    /// Verifies if Stream table has Version column of type Long
    /// </summary>
    [Fact]
    public void EventsTable_ShouldHave_VersionColumn_WithLongType()
    {
        var versionColumn = schemaProvider
            .GetTable(EventsTableName)?
            .GetColumn(VersionColumnName);

        versionColumn.ShouldNotBeNull();
        versionColumn!.Name.ShouldBe(VersionColumnName);
        versionColumn.Type.ShouldBe(Column.LongType);
    }

    /// <summary>
    /// Verifies if Stream table has Version column of type Long
    /// </summary>
    [Fact]
    public void EventsTable_ShouldHave_CreatedColumn_WithDateTimeType()
    {
        var createdColumn = schemaProvider
            .GetTable(EventsTableName)?
            .GetColumn(CreatedColumnName);

        createdColumn.ShouldNotBeNull();
        createdColumn!.Name.ShouldBe(CreatedColumnName);
        createdColumn.Type.ShouldBe(Column.DateTimeType);
    }

    public ValueTask InitializeAsync() =>
        new(eventStore.Init());

    public async ValueTask DisposeAsync() =>
        await eventStore.DisposeAsync();
}
