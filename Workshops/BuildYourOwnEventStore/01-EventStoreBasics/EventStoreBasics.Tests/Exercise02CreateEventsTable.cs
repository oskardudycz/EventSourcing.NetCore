using System;
using EventStoreBasics.Tests.Tools;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace EventStoreBasics.Tests
{
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
    public class Exercise02CreateEventsTable : IDisposable
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
        public void EventsTable_ShouldHave_DataColumn_WithJsonType()
        {
            var dataColumn = schemaProvider
                .GetTable(EventsTableName)?
                .GetColumn(DataColumnName);

            dataColumn.Should().NotBeNull();
            dataColumn!.Name.Should().Be(DataColumnName);
            dataColumn.Type.Should().Be(Column.JsonType);
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

            dataColumn.Should().NotBeNull();
            dataColumn!.Name.Should().Be(StreamIdColumnName);
            dataColumn.Type.Should().Be(Column.GuidType);
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

            typeColumn.Should().NotBeNull();
            typeColumn!.Name.Should().Be(TypeColumnName);
            typeColumn.Type.Should().Be(Column.StringType);
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

            versionColumn.Should().NotBeNull();
            versionColumn!.Name.Should().Be(VersionColumnName);
            versionColumn.Type.Should().Be(Column.LongType);
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
}
