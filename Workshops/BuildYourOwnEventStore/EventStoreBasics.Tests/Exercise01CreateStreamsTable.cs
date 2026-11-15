using Shouldly;
using Tools.Tools;
using Xunit;

namespace EventStoreBasics.Tests;

/// <summary>
/// Exercise 01 - Create Streams Table
/// </summary>
/// <remarks>
/// <para>
/// Stream represents ordered set of events. Simply speaking stream is an log of the events that happened for the
/// specific aggregate/entity.
/// </para>
///
/// <para>
/// As truth is in the log, so in the events - stream can be also interpreted as the grouping, where key is stream id.
/// </para>
///
/// <para>
/// <c>Id</c> needs to be unique, so normally is represented by Guid type.
/// </para>
///
/// <para>
/// It's also common to add two other columns:
/// </para>
/// <list type="bullet">
/// <item>
///     <term><c>Type</c></term>
///     <description>information about the stream type. It' mostly used to make debugging easier or some optimizations.</description>
/// </item>
/// <item>
///     <term><c>Version</c></term>
///     <description>auto-incremented value used mostly for optimistic concurrency check</description>
/// </item>
/// </list>
/// <para>
/// Class provides set of tests verifying if <see cref="EventStore.Init()"/> method initializes <c>Streams</c> table properly.
/// </para>
/// </remarks>
public class Exercise01CreateStreamsTable : IAsyncLifetime
{
    private readonly PostgresSchemaProvider schemaProvider;
    private readonly EventStore eventStore;

    private const string StreamsTableName = "streams";

    private const string IdColumnName = "id";
    private const string TypeColumnName = "type";
    private const string VersionColumnName = "version";

    /// <summary>
    /// Inits Event Store
    /// </summary>
    public Exercise01CreateStreamsTable()
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
    public void StreamsTable_ShouldBeCreated()
    {
        var streamsTable = schemaProvider.GetTable(StreamsTableName);

        streamsTable.ShouldNotBeNull();
        streamsTable!.Name.ShouldBe(StreamsTableName);
    }

    /// <summary>
    /// Verifies if Stream table has Id column of type Guid
    /// </summary>
    [Fact]
    public void StreamsTable_ShouldHave_IdColumn()
    {
        var idColumn = schemaProvider
            .GetTable(StreamsTableName)?
            .GetColumn(IdColumnName);

        idColumn.ShouldNotBeNull();
        idColumn!.Name.ShouldBe(IdColumnName);
        idColumn.Type.ShouldBe(Column.GuidType);
    }

    /// <summary>
    /// Verifies if Stream table has Type column of type String
    /// </summary>
    [Fact]
    public void StreamsTable_ShouldHave_TypeColumn_WithStringType()
    {
        var typeColumn = schemaProvider
            .GetTable(StreamsTableName)?
            .GetColumn(TypeColumnName);

        typeColumn.ShouldNotBeNull();
        typeColumn!.Name.ShouldBe(TypeColumnName);
        typeColumn.Type.ShouldBe(Column.StringType);
    }

    /// <summary>
    /// Verifies if Stream table has Version column of type Long
    /// </summary>
    [Fact]
    public void StreamsTable_ShouldHave_VersionColumn_WithLongType()
    {
        var versionColumn = schemaProvider
            .GetTable(StreamsTableName)?
            .GetColumn(VersionColumnName);

        versionColumn.ShouldNotBeNull();
        versionColumn!.Name.ShouldBe(VersionColumnName);
        versionColumn.Type.ShouldBe(Column.LongType);
    }

    public ValueTask InitializeAsync() =>
        new(eventStore.Init());

    public async ValueTask DisposeAsync() =>
        await eventStore.DisposeAsync();
}
