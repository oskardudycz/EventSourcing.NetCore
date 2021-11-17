using System;
using EventStoreBasics.Tests.Tools;
using FluentAssertions;
using Npgsql;
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

        streamsTable.Should().NotBeNull();
        streamsTable!.Name.Should().Be(EventsTableName);
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

        idColumn.Should().NotBeNull();
        idColumn!.Name.Should().Be(IdColumnName);
        idColumn.Type.Should().Be(Column.GuidType);
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

        dataColumn.Should().NotBeNull();
        dataColumn!.Name.Should().Be(DataColumnName);
        dataColumn!.Type.Should().Be(Column.JsonType);
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

        dataColumn.Should().NotBeNull();
        dataColumn!.Name.Should().Be(StreamIdColumnName);
        dataColumn.Type.Should().Be(Column.GuidType);
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

        typeColumn.Should().NotBeNull();
        typeColumn!.Name.Should().Be(TypeColumnName);
        typeColumn.Type.Should().Be(Column.StringType);
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

        versionColumn.Should().NotBeNull();
        versionColumn!.Name.Should().Be(VersionColumnName);
        versionColumn.Type.Should().Be(Column.LongType);
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

        createdColumn.Should().NotBeNull();
        createdColumn!.Name.Should().Be(CreatedColumnName);
        createdColumn.Type.Should().Be(Column.DateTimeType);
    }

    /// <summary>
    /// Disposes connection to database
    /// </summary>
    public void Dispose()
    {
        databaseConnection.Dispose();
    }
}