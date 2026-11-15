using Shouldly;
using Npgsql;
using Tools.Tools;
using Xunit;

namespace EventStoreBasics.Tests;

public class Exercise02CreateEventsTable: IDisposable
{
    private readonly NpgsqlConnection databaseConnection;
    private readonly PostgresSchemaProvider schemaProvider;

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
        databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection();
        schemaProvider = new PostgresSchemaProvider(databaseConnection);

        // Create Event Store
        var eventStore = new EventStore(databaseConnection);

        // Initialize Event Store
        eventStore.Init();
    }

    /// <summary>
    /// Verifies if Stream table was created
    /// </summary>
    [Fact]
    [Trait("Category", "SkipCI")]
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
    [Trait("Category", "SkipCI")]
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
    [Trait("Category", "SkipCI")]
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
    [Trait("Category", "SkipCI")]
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
    [Trait("Category", "SkipCI")]
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
    [Trait("Category", "SkipCI")]
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
    [Trait("Category", "SkipCI")]
    public void EventsTable_ShouldHave_CreatedColumn_WithDateTimeType()
    {
        var createdColumn = schemaProvider
            .GetTable(EventsTableName)?
            .GetColumn(CreatedColumnName);

        createdColumn.ShouldNotBeNull();
        createdColumn!.Name.ShouldBe(CreatedColumnName);
        createdColumn.Type.ShouldBe(Column.DateTimeType);
    }

    /// <summary>
    /// Disposes connection to database
    /// </summary>
    public void Dispose() =>
        databaseConnection.Dispose();
}
